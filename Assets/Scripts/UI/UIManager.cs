using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HereToSlay.Core;
using HereToSlay.Networking;

namespace HereToSlay.UI
{
    /// <summary>
    /// Client-side board renderer. Listens to GameManager.OnStateChanged and redraws.
    /// Place on a single GameObject in the scene; wire all UI fields in Inspector.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // ── Inspector refs ────────────────────────────────────────────────────
        [Header("Card Prefabs")]
        [SerializeField] GameObject heroCardPrefab;
        [SerializeField] GameObject monsterCardPrefab;
        [SerializeField] GameObject magicItemCardPrefab;
        [SerializeField] GameObject modifierCardPrefab;
        [SerializeField] GameObject partyLeaderPrefab;

        [Header("Panels")]
        [SerializeField] Transform  playerHandArea;
        [SerializeField] Transform  playerPartyArea;
        [SerializeField] Transform  monsterRowArea;
        [SerializeField] Transform  opponentListArea;   // scrollable list of opponent panels
        [SerializeField] Transform  discardArea;

        [Header("HUD")]
        [SerializeField] TextMeshProUGUI  actionsLabel;
        [SerializeField] TextMeshProUGUI  phaseLabel;
        [SerializeField] TextMeshProUGUI  activePlayerLabel;
        [SerializeField] TextMeshProUGUI  deckCountLabel;

        [Header("Dice Popup")]
        [SerializeField] GameObject  dicePopupPanel;
        [SerializeField] TextMeshProUGUI diceRollText;
        [SerializeField] TextMeshProUGUI diceResultText;

        [Header("Reaction Bar")]
        [SerializeField] GameObject  reactionBar;
        [SerializeField] Button      passReactionBtn;

        [Header("Action Buttons")]
        [SerializeField] Button      drawCardBtn;
        [SerializeField] Button      endTurnBtn;

        [Header("Win Screen")]
        [SerializeField] GameObject  winScreen;
        [SerializeField] TextMeshProUGUI winnerLabel;

        [Header("Opponent Panel Prefab")]
        [SerializeField] GameObject  opponentPanelPrefab;

        // ── State cache ───────────────────────────────────────────────────────
        GameState    _latestState;
        string       _localPlayerId;
        PlayerState  _localPlayerState;

        // ── Card selection ────────────────────────────────────────────────────
        CardUIItem   _selectedCard;
        CardUIItem   _selectedTarget;

        // ════════════════════════════════════════════════════════════════════

        void Awake()
        {
            GameManager.OnStateChanged += HandleStateUpdate;
            GameManager.OnDiceRolled   += HandleDiceRoll;
            GameManager.OnGameOver     += HandleGameOver;
        }

        void OnDestroy()
        {
            GameManager.OnStateChanged -= HandleStateUpdate;
            GameManager.OnDiceRolled   -= HandleDiceRoll;
            GameManager.OnGameOver     -= HandleGameOver;
        }

        void Start()
        {
            dicePopupPanel.SetActive(false);
            reactionBar.SetActive(false);
            winScreen.SetActive(false);

            drawCardBtn.onClick.AddListener(() =>
                NetworkPlayerController.LocalInstance?.CmdDrawCard());
            endTurnBtn.onClick.AddListener(() =>
                NetworkPlayerController.LocalInstance?.CmdEndTurn());
            passReactionBtn.onClick.AddListener(() =>
                NetworkPlayerController.LocalInstance?.CmdPassReaction());
        }

        // ════════════════════════════════════════════════════════════════════
        //  STATE UPDATE
        // ════════════════════════════════════════════════════════════════════

        void HandleStateUpdate(GameState state)
        {
            _latestState = state;
            _localPlayerId = NetworkPlayerController.LocalInstance?.playerId;
            _localPlayerState = state.GetPlayer(_localPlayerId);

            RedrawHUD(state);
            RedrawMonsterRow(state);
            RedrawLocalHand(state);
            RedrawLocalParty(state);
            RedrawOpponents(state);
            UpdateActionButtons(state);
            UpdateReactionBar(state);
        }

        // ── HUD ──────────────────────────────────────────────────────────────

        void RedrawHUD(GameState state)
        {
            var active = state.GetActivePlayer();
            activePlayerLabel.text = $"Active: {active.playerName}";
            phaseLabel.text        = state.phase.ToString();
            actionsLabel.text      = $"Actions: {state.actionsRemaining}";
            deckCountLabel.text    = $"Deck: {state.mainDeck.Count}";
        }

        // ── Monster row ───────────────────────────────────────────────────────

        void RedrawMonsterRow(GameState state)
        {
            ClearChildren(monsterRowArea);
            foreach (var card in state.monsterRow)
            {
                var go = Instantiate(monsterCardPrefab, monsterRowArea);
                var ui = go.GetComponent<CardUIItem>();
                ui.Populate(card, CardZone.MonsterRow);
                ui.OnClicked += OnMonsterClicked;
            }
        }

        // ── Local player ──────────────────────────────────────────────────────

        void RedrawLocalHand(GameState state)
        {
            ClearChildren(playerHandArea);
            if (_localPlayerState == null) return;

            foreach (var card in _localPlayerState.hand)
            {
                var go = Instantiate(GetPrefabFor(card), playerHandArea);
                var ui = go.GetComponent<CardUIItem>();
                ui.Populate(card, CardZone.Hand);
                ui.OnClicked += OnHandCardClicked;
            }
        }

        void RedrawLocalParty(GameState state)
        {
            ClearChildren(playerPartyArea);
            if (_localPlayerState == null) return;

            if (_localPlayerState.partyLeader != null)
            {
                var leaderGo = Instantiate(partyLeaderPrefab, playerPartyArea);
                leaderGo.GetComponent<CardUIItem>()
                        .Populate(_localPlayerState.partyLeader, CardZone.Party);
            }

            foreach (var card in _localPlayerState.party)
            {
                var go = Instantiate(heroCardPrefab, playerPartyArea);
                var ui = go.GetComponent<CardUIItem>();
                ui.Populate(card, CardZone.Party);
                ui.OnClicked += OnPartyHeroClicked;
            }
        }

        // ── Opponents ─────────────────────────────────────────────────────────

        void RedrawOpponents(GameState state)
        {
            ClearChildren(opponentListArea);
            foreach (var p in state.players)
            {
                if (p.playerId == _localPlayerId) continue;
                var panelGo = Instantiate(opponentPanelPrefab, opponentListArea);
                panelGo.GetComponent<OpponentPanel>()?.Populate(p, state);
            }
        }

        // ── Buttons & reaction bar ────────────────────────────────────────────

        void UpdateActionButtons(GameState state)
        {
            bool isMyTurn   = state.IsActiveTurn(_localPlayerId);
            bool hasActions = state.actionsRemaining > 0;

            drawCardBtn.interactable = isMyTurn && hasActions;
            endTurnBtn.interactable  = isMyTurn;
        }

        void UpdateReactionBar(GameState state)
        {
            bool canReact = state.CanReact(_localPlayerId);
            reactionBar.SetActive(canReact);
        }

        // ════════════════════════════════════════════════════════════════════
        //  CLICK HANDLERS
        // ════════════════════════════════════════════════════════════════════

        void OnHandCardClicked(CardUIItem item)
        {
            // If in reaction window and card is a modifier, play it immediately
            if (_latestState?.phase == GamePhase.ReactionWindow &&
                item.Card.cardType == CardType.Modifier)
            {
                // Determine if it targets self or opponent (hero challenge)
                bool targetsSelf = item.Card.modifierData?.targetsOwner ?? false;
                NetworkPlayerController.LocalInstance?
                    .CmdPlayModifier(item.Card.instanceId, targetsSelf);
                return;
            }

            // Otherwise select card for playing
            SelectCard(item);
        }

        void OnPartyHeroClicked(CardUIItem item)
        {
            if (!_latestState.IsActiveTurn(_localPlayerId)) return;

            // Activate hero ability if no card selected; otherwise pick target
            if (_selectedCard == null)
                NetworkPlayerController.LocalInstance?
                    .CmdUseHeroAbility(item.Card.instanceId, "", "");
            else
                SetTarget(item);
        }

        void OnMonsterClicked(CardUIItem item)
        {
            if (!_latestState.IsActiveTurn(_localPlayerId)) return;

            // Direct monster slay attempt
            NetworkPlayerController.LocalInstance?
                .CmdChallengeMonster(item.Card.instanceId);
        }

        void SelectCard(CardUIItem item)
        {
            _selectedCard?.SetHighlight(false);
            _selectedCard = item;
            _selectedCard?.SetHighlight(true);
        }

        void SetTarget(CardUIItem item)
        {
            if (_selectedCard == null) return;
            _selectedTarget = item;

            NetworkPlayerController.LocalInstance?.CmdPlayCard(
                _selectedCard.Card.instanceId,
                _selectedTarget.Card.instanceId,
                "");

            _selectedCard?.SetHighlight(false);
            _selectedCard   = null;
            _selectedTarget = null;
        }

        // ════════════════════════════════════════════════════════════════════
        //  DICE POPUP
        // ════════════════════════════════════════════════════════════════════

        void HandleDiceRoll(string playerId, DiceRollResult result)
        {
            dicePopupPanel.SetActive(true);
            diceRollText.text = $"🎲 Rolled: {result.rawRoll}  (need {result.requiredRoll}+)";
            string modStr = result.totalModifier != 0
                ? $"  Modifier: {result.totalModifier:+#;-#;0}  →  Final: {result.finalRoll}"
                : "";
            diceResultText.text = result.success
                ? $"<color=#44ff44>✔ SUCCESS{modStr}</color>"
                : $"<color=#ff4444>✘ FAILURE{modStr}</color>";

            StartCoroutine(HideDicePopupAfter(3f));
        }

        IEnumerator HideDicePopupAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            dicePopupPanel.SetActive(false);
        }

        // ════════════════════════════════════════════════════════════════════
        //  GAME OVER
        // ════════════════════════════════════════════════════════════════════

        void HandleGameOver(string winnerId)
        {
            winScreen.SetActive(true);
            var winner = _latestState?.GetPlayer(winnerId);
            winnerLabel.text = winner != null
                ? $"🏆 {winner.playerName} WINS!"
                : "🏆 Game Over!";
        }

        // ════════════════════════════════════════════════════════════════════
        //  UTILS
        // ════════════════════════════════════════════════════════════════════

        static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        GameObject GetPrefabFor(CardInstance card) => card.cardType switch
        {
            CardType.Hero       => heroCardPrefab,
            CardType.Monster    => monsterCardPrefab,
            CardType.MagicItem  => magicItemCardPrefab,
            CardType.Modifier   => modifierCardPrefab,
            CardType.PartyLeader=> partyLeaderPrefab,
            _                   => heroCardPrefab
        };
    }

    public enum CardZone { Hand, Party, MonsterRow, Discard }
}
