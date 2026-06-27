using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using HereToSlay.Core;
using HereToSlay.Cards;
using HereToSlay.Effects;

namespace HereToSlay.Networking
{
    /// <summary>
    /// Authoritative server-side game manager. Lives on the server only.
    /// Clients receive state updates through SyncVars and ClientRpc calls.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Config")]
        [SerializeField] bool useExpansions = true;
        [SerializeField] int  startingHandSize = 5;

        // ── State ────────────────────────────────────────────────────────────
        GameState _state = new();
        EffectResolver _effectResolver;

        // ── Events (server → all clients) ───────────────────────────────────
        public static event Action<GameState> OnStateChanged;
        public static event Action<string, DiceRollResult> OnDiceRolled;   // playerId, result
        public static event Action<string>    OnGameOver;                   // winnerId

        // ────────────────────────────────────────────────────────────────────

        void Awake()
        {
            Instance = this;
            _effectResolver = new EffectResolver();
        }

        // ════════════════════════════════════════════════════════════════════
        //  LOBBY & SETUP
        // ════════════════════════════════════════════════════════════════════

        [Server]
        public void RegisterPlayer(NetworkConnection conn, string playerName)
        {
            if (_state.phase != GamePhase.Lobby)
            {
                Debug.LogWarning("Game already started; ignoring late join.");
                return;
            }
            if (_state.players.Count >= 6)
            {
                Debug.LogWarning("Max players reached.");
                return;
            }

            var ps = new PlayerState
            {
                playerId    = conn.connectionId.ToString(),
                playerName  = playerName,
                playerIndex = _state.players.Count
            };
            _state.players.Add(ps);
            BroadcastState();
        }

        [Server]
        public void PlayerReady(string playerId)
        {
            var p = _state.GetPlayer(playerId);
            if (p == null) return;
            p.isReady = true;

            bool allReady = _state.players.Count >= 2 &&
                            _state.players.TrueForAll(x => x.isReady);
            if (allReady) StartGame();
        }

        [Server]
        void StartGame()
        {
            _state.phase        = GamePhase.Setup;
            _state.useExpansions = useExpansions;
            _state.heroClassesRequired = useExpansions ? 10 : 6;

            BuildDecks();
            DealStartingHands();
            AssignPartyLeaders();
            RefillMonsterRow();

            _state.activePlayerIndex = 0;
            _state.actionsRemaining  = GameState.ActionsPerTurn;
            _state.phase             = GamePhase.PlayerTurn;
            BroadcastState();
        }

        // ────────────────────────────────────────────────────────────────────

        [Server]
        void BuildDecks()
        {
            var mainCards = new List<CardInstance>();

            void Add(IEnumerable<CardDefinition> defs)
            {
                foreach (var def in defs)
                {
                    int copies = CardCatalogue.CopiesOf(def);
                    for (int i = 0; i < copies; i++)
                        mainCards.Add(CardInstance.Create(def.cardType, def.assetName));
                }
            }

            Add(CardCatalogue.BaseHeroes);
            Add(CardCatalogue.MagicItems);
            Add(CardCatalogue.Modifiers);

            if (useExpansions)
            {
                Add(CardCatalogue.ExpansionHeroes);
            }

            Shuffle(mainCards);
            _state.mainDeck = mainCards;

            // Monster deck
            var monsters = new List<CardInstance>();
            foreach (var def in CardCatalogue.BaseMonsters)
                monsters.Add(CardInstance.Create(CardType.Monster, def.assetName));
            if (useExpansions)
                foreach (var def in CardCatalogue.ExpansionMonsters)
                    monsters.Add(CardInstance.Create(CardType.Monster, def.assetName));
            Shuffle(monsters);
            _state.monsterDeck = monsters;
        }

        [Server]
        void DealStartingHands()
        {
            foreach (var p in _state.players)
            {
                for (int i = 0; i < startingHandSize; i++)
                    DrawCard(p);
            }
        }

        [Server]
        void AssignPartyLeaders()
        {
            // Assign a random unused leader to each player
            var leaders = new List<CardDefinition>(CardCatalogue.PartyLeaders);
            Shuffle(leaders);
            for (int i = 0; i < _state.players.Count && i < leaders.Count; i++)
            {
                _state.players[i].partyLeader =
                    CardInstance.Create(CardType.PartyLeader, leaders[i].assetName);
            }
        }

        [Server]
        void RefillMonsterRow()
            => _state.ReplenishMonsterRow(_state.monsterDeck);

        // ════════════════════════════════════════════════════════════════════
        //  ACTION PROCESSING  (called from NetworkPlayerController → server)
        // ════════════════════════════════════════════════════════════════════

        [Server]
        public void ProcessAction(GameAction action)
        {
            if (_state.phase == GamePhase.GameOver) return;

            switch (action.actionType)
            {
                case ActionType.PlayCard:       HandlePlayCard(action);       break;
                case ActionType.UseHeroAbility: HandleHeroAbility(action);    break;
                case ActionType.ChallengeMonster: HandleChallengeMonster(action); break;
                case ActionType.DrawCard:       HandleDrawCard(action);       break;
                case ActionType.EndTurn:        HandleEndTurn(action);        break;
                case ActionType.PlayModifier:   HandlePlayModifier(action);   break;
                case ActionType.PassReaction:   HandlePassReaction(action);   break;
            }
        }

        // ── Play a card from hand ─────────────────────────────────────────

        [Server]
        void HandlePlayCard(GameAction action)
        {
            if (!ValidateActiveTurn(action)) return;

            var player = _state.GetActivePlayer();
            if (!player.RemoveFromHand(action.cardInstanceId, out var card)) return;

            switch (card.cardType)
            {
                case CardType.Hero:
                    player.AddHeroToParty(card);
                    _effectResolver.ApplyEffects(card, EffectTrigger.OnPlay, action, _state);
                    break;
                case CardType.MagicItem:
                    var itemDef = FindMagicItemDef(card.dataAssetName);
                    if (itemDef != null && !itemDef.isPersistent)
                    {
                        // One-time: resolve effect then discard
                        _effectResolver.ApplyEffects(card, EffectTrigger.OnPlay, action, _state);
                        _state.discardPile.Add(card);
                    }
                    else
                    {
                        player.magicItems.Add(card);
                        _effectResolver.ApplyEffects(card, EffectTrigger.OnPlay, action, _state);
                    }
                    break;
                default:
                    _state.discardPile.Add(card);
                    break;
            }

            ConsumeAction(player);
            CheckWin();
            BroadcastState();
        }

        // ── Activate a hero ability ───────────────────────────────────────

        [Server]
        void HandleHeroAbility(GameAction action)
        {
            if (!ValidateActiveTurn(action)) return;
            var player = _state.GetActivePlayer();

            // Find the hero in party
            CardInstance hero = null;
            foreach (var c in player.party)
                if (c.instanceId == action.cardInstanceId) { hero = c; break; }
            if (hero == null) return;

            // Roll dice for ability
            int roll = RollD10();
            var def = FindHeroDef(hero.dataAssetName);
            int required = def?.rollRequirement ?? 5;

            bool success = roll >= required;
            RpcNotifyDiceRoll(player.playerId, roll, required, success, "Hero Ability");

            if (success)
                _effectResolver.ApplyEffects(hero, EffectTrigger.OnAction, action, _state);

            ConsumeAction(player);
            BroadcastState();
        }

        // ── Challenge a monster ───────────────────────────────────────────

        [Server]
        void HandleChallengeMonster(GameAction action)
        {
            if (!ValidateActiveTurn(action)) return;
            if (_state.FindCardInMonsterRow(action.targetInstanceId) == null) return;

            _state.phase                      = GamePhase.MonsterChallenge;
            _state.challengePlayerId          = action.actingPlayerId;
            _state.challengeMonsterInstanceId = action.targetInstanceId;

            // Seed the roll — players may modify before resolution
            int raw = RollD10();
            _state.pendingRoll = new DiceRollResult
            {
                rawRoll     = raw,
                finalRoll   = raw,
                requiredRoll = GetMonsterRollRequirement(action.targetInstanceId)
            };

            // Open reaction window for all OTHER players
            _state.playersYetToReact.Clear();
            foreach (var p in _state.players)
                if (p.playerId != action.actingPlayerId)
                    _state.playersYetToReact.Add(p.playerId);

            _state.phase = GamePhase.ReactionWindow;
            ConsumeAction(_state.GetActivePlayer());
            BroadcastState();
        }

        // ── Modifier played during reaction window ────────────────────────

        [Server]
        void HandlePlayModifier(GameAction action)
        {
            if (_state.phase != GamePhase.ReactionWindow) return;
            var player = _state.GetPlayer(action.actingPlayerId);
            if (player == null) return;

            if (!player.RemoveFromHand(action.cardInstanceId, out var card)) return;
            if (card.cardType != CardType.Modifier) return;

            var def = FindModifierDef(card.dataAssetName);
            if (def != null)
            {
                if (def.isForceReroll)
                    _state.pendingRoll.finalRoll = RollD10();
                else
                    _state.pendingRoll.totalModifier += def.rollModifier;

                _state.pendingRoll.appliedModifierIds.Add(card.instanceId);
            }

            _state.discardPile.Add(card);
            _state.playersYetToReact.Remove(action.actingPlayerId);

            if (_state.playersYetToReact.Count == 0)
                ResolveMonsterChallenge();
            else
                BroadcastState();
        }

        // ── Pass during reaction window ───────────────────────────────────

        [Server]
        void HandlePassReaction(GameAction action)
        {
            _state.playersYetToReact.Remove(action.actingPlayerId);
            if (_state.playersYetToReact.Count == 0)
                ResolveMonsterChallenge();
            else
                BroadcastState();
        }

        // ── Draw card ─────────────────────────────────────────────────────

        [Server]
        void HandleDrawCard(GameAction action)
        {
            if (!ValidateActiveTurn(action)) return;
            var player = _state.GetActivePlayer();
            DrawCard(player);
            ConsumeAction(player);
            BroadcastState();
        }

        // ── End turn ─────────────────────────────────────────────────────

        [Server]
        void HandleEndTurn(GameAction action)
        {
            if (!ValidateActiveTurn(action)) return;
            _state.AdvanceTurn();
            // Active player draws 1 card at start of turn
            DrawCard(_state.GetActivePlayer());
            BroadcastState();
        }

        // ════════════════════════════════════════════════════════════════════
        //  MONSTER CHALLENGE RESOLUTION
        // ════════════════════════════════════════════════════════════════════

        [Server]
        void ResolveMonsterChallenge()
        {
            _state.phase = GamePhase.ResolveDice;

            var roll = _state.pendingRoll;
            roll.finalRoll = roll.rawRoll + roll.totalModifier;
            roll.finalRoll = Mathf.Clamp(roll.finalRoll, 1, 10);
            roll.success   = roll.finalRoll >= roll.requiredRoll;

            var challenger = _state.GetPlayer(_state.challengePlayerId);
            RpcNotifyDiceRoll(challenger.playerId, roll.rawRoll, roll.requiredRoll,
                              roll.success, $"Slay (mod {roll.totalModifier:+#;-#;0})");

            if (roll.success)
            {
                challenger.monstersSlain++;

                // Remove monster from row and apply reward
                CardInstance monster = null;
                for (int i = 0; i < _state.monsterRow.Count; i++)
                {
                    if (_state.monsterRow[i].instanceId == _state.challengeMonsterInstanceId)
                    {
                        monster = _state.monsterRow[i];
                        _state.monsterRow.RemoveAt(i);
                        break;
                    }
                }
                if (monster != null)
                {
                    _state.discardPile.Add(monster);
                    _effectResolver.ApplyMonsterReward(monster, challenger, _state);
                }

                RefillMonsterRow();
            }

            _state.phase = GamePhase.PlayerTurn;
            _state.challengePlayerId = null;
            _state.challengeMonsterInstanceId = null;
            _state.pendingRoll = null;

            CheckWin();
            BroadcastState();
        }

        // ════════════════════════════════════════════════════════════════════
        //  UTILITIES
        // ════════════════════════════════════════════════════════════════════

        [Server]
        void DrawCard(PlayerState player, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (_state.mainDeck.Count == 0)
                {
                    // Reshuffle discard
                    _state.mainDeck = new List<CardInstance>(_state.discardPile);
                    _state.discardPile.Clear();
                    Shuffle(_state.mainDeck);
                }
                if (_state.mainDeck.Count == 0) break;
                player.hand.Add(_state.mainDeck[0]);
                _state.mainDeck.RemoveAt(0);
            }
        }

        [Server]
        void ConsumeAction(PlayerState player)
        {
            _state.actionsRemaining--;
            if (_state.actionsRemaining <= 0)
            {
                _state.AdvanceTurn();
                DrawCard(_state.GetActivePlayer());
            }
        }

        [Server]
        void CheckWin()
        {
            if (_state.CheckWinCondition(out string winnerId))
            {
                _state.winnerPlayerId = winnerId;
                _state.phase = GamePhase.GameOver;
                RpcGameOver(winnerId);
            }
        }

        [Server]
        bool ValidateActiveTurn(GameAction action)
        {
            if (_state.phase != GamePhase.PlayerTurn) return false;
            if (_state.GetActivePlayer().playerId != action.actingPlayerId) return false;
            if (_state.actionsRemaining <= 0) return false;
            return true;
        }

        [Server]
        int GetMonsterRollRequirement(string instanceId)
        {
            var card = _state.FindCardInMonsterRow(instanceId);
            if (card == null) return 99;
            var def = FindMonsterDef(card.dataAssetName);
            return def?.rollToSlay ?? 7;
        }

        static int RollD10() => UnityEngine.Random.Range(1, 11);

        static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // ── Definition lookups ────────────────────────────────────────────

        static CardDefinition FindHeroDef(string assetName)
        {
            foreach (var d in CardCatalogue.BaseHeroes)
                if (d.assetName == assetName) return d;
            foreach (var d in CardCatalogue.ExpansionHeroes)
                if (d.assetName == assetName) return d;
            return null;
        }

        static CardDefinition FindMonsterDef(string assetName)
        {
            foreach (var d in CardCatalogue.BaseMonsters)
                if (d.assetName == assetName) return d;
            foreach (var d in CardCatalogue.ExpansionMonsters)
                if (d.assetName == assetName) return d;
            return null;
        }

        static MagicItemCardData FindMagicItemDef(string assetName)
            => Resources.Load<MagicItemCardData>($"Cards/Items/{assetName}");

        static ModifierCardData FindModifierDef(string assetName)
            => Resources.Load<ModifierCardData>($"Cards/Modifiers/{assetName}");

        // ════════════════════════════════════════════════════════════════════
        //  BROADCAST  (server → all clients)
        // ════════════════════════════════════════════════════════════════════

        [Server]
        void BroadcastState()
        {
            // Serialize to JSON and send; clients deserialize and update UI
            string json = JsonUtility.ToJson(_state);
            RpcReceiveState(json);
        }

        [ClientRpc]
        void RpcReceiveState(string json)
        {
            var state = JsonUtility.FromJson<GameState>(json);
            OnStateChanged?.Invoke(state);
        }

        [ClientRpc]
        void RpcNotifyDiceRoll(string playerId, int rawRoll, int required, bool success, string context)
        {
            var result = new DiceRollResult
            {
                rawRoll = rawRoll, requiredRoll = required, success = success
            };
            OnDiceRolled?.Invoke(playerId, result);
        }

        [ClientRpc]
        void RpcGameOver(string winnerId)
        {
            OnGameOver?.Invoke(winnerId);
        }
    }
}
