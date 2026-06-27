using System.Collections.Generic;
using UnityEngine;
using HereToSlay.Core;

namespace HereToSlay.Effects
{
    /// <summary>
    /// Resolves all CardEffect payloads on the server.
    /// Called by GameManager after validating the action context.
    /// </summary>
    public class EffectResolver
    {
        // Injected by GameManager when effects need to draw cards
        System.Action<PlayerState, int> _drawCards;
        System.Action<PlayerState>      _refillMonsterRow;

        public void SetCallbacks(
            System.Action<PlayerState, int> drawCards,
            System.Action<PlayerState>      refillMonsterRow)
        {
            _drawCards        = drawCards;
            _refillMonsterRow = refillMonsterRow;
        }

        // ────────────────────────────────────────────────────────────────────
        //  HERO / ITEM EFFECTS
        // ────────────────────────────────────────────────────────────────────

        public void ApplyEffects(CardInstance card, EffectTrigger trigger,
                                 GameAction action, GameState state)
        {
            List<CardEffect> effects = null;

            if (card.heroData      != null) effects = card.heroData.effects;
            else if (card.magicItemData != null) effects = card.magicItemData.effects;
            else if (card.leaderData    != null) effects = card.leaderData.passiveEffects;
            if (effects == null) return;

            var actor = state.GetPlayer(action.actingPlayerId);
            if (actor == null) return;

            foreach (var fx in effects)
            {
                if (fx.trigger != trigger) continue;
                Resolve(fx, actor, action, state);
            }
        }

        public void ApplyMonsterReward(CardInstance monster, PlayerState slayer, GameState state)
        {
            if (monster.monsterData?.rewardEffects == null) return;
            var fakeAction = new GameAction { actingPlayerId = slayer.playerId };
            foreach (var fx in monster.monsterData.rewardEffects)
                Resolve(fx, slayer, fakeAction, state);
        }

        // ────────────────────────────────────────────────────────────────────

        void Resolve(CardEffect fx, PlayerState actor, GameAction action, GameState state)
        {
            PlayerState target = ResolveTargetPlayer(fx.target, actor, action, state);

            switch (fx.effectType)
            {
                // ── Card draw / discard ──────────────────────────────────────
                case EffectType.DrawCards:
                    _drawCards?.Invoke(actor, fx.value);
                    break;

                case EffectType.DiscardCards:
                    if (target != null)
                        ForceDiscard(target, fx.value, state);
                    break;

                // ── Hero manipulation ────────────────────────────────────────
                case EffectType.StealHero:
                    StealHero(actor, action.targetInstanceId, state);
                    break;

                case EffectType.DestroyHero:
                    DestroyHero(action.targetInstanceId, state);
                    break;

                case EffectType.ReturnHero:
                    ReturnHeroToHand(action.targetInstanceId, state);
                    break;

                case EffectType.AddHeroToParty:
                    // Pull from discard into actor's party
                    PullHeroFromDiscard(action.targetInstanceId, actor, state);
                    break;

                case EffectType.SwapHero:
                    SwapHeroes(actor, action.targetPlayerId, action.targetInstanceId,
                               action.cardInstanceId, state);
                    break;

                // ── Protection ───────────────────────────────────────────────
                case EffectType.ProtectHero:
                    if (target != null) target.isProtected = true;
                    else actor.isProtected = true;
                    break;

                // ── Item manipulation ────────────────────────────────────────
                case EffectType.StealMagicItem:
                    StealMagicItem(actor, action.targetInstanceId, state);
                    break;

                case EffectType.DestroyMagicItem:
                    DestroyMagicItem(action.targetInstanceId, state);
                    break;

                // ── Dice modifiers (applied in ReactionWindow, not here) ─────
                case EffectType.RollBonus:
                    if (state.pendingRoll != null)
                        state.pendingRoll.totalModifier += fx.value;
                    break;

                case EffectType.RollPenalty:
                    if (state.pendingRoll != null)
                        state.pendingRoll.totalModifier -= fx.value;
                    break;

                case EffectType.ForceReroll:
                    if (state.pendingRoll != null)
                        state.pendingRoll.rawRoll = UnityEngine.Random.Range(1, 11);
                    break;

                // ── Actions ──────────────────────────────────────────────────
                case EffectType.GainActions:
                    state.actionsRemaining += fx.value;
                    break;

                // ── Deck manipulation ────────────────────────────────────────
                case EffectType.LookAtHand:
                    // Handled client-side via a targeted RPC (server sends hand to requesting player)
                    break;

                // ── Victory ──────────────────────────────────────────────────
                case EffectType.SlayMonsterBonus:
                    actor.monstersSlain += fx.value;
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPER OPERATIONS
        // ════════════════════════════════════════════════════════════════════

        void ForceDiscard(PlayerState target, int count, GameState state)
        {
            int actual = Mathf.Min(count, target.hand.Count);
            for (int i = 0; i < actual; i++)
            {
                // Target discards from the END of hand (simulates player choice;
                // for full impl, send a TargetedRPC to client to choose)
                var card = target.hand[target.hand.Count - 1];
                target.hand.RemoveAt(target.hand.Count - 1);
                state.discardPile.Add(card);
            }
        }

        void StealHero(PlayerState thief, string heroInstanceId, GameState state)
        {
            if (state.FindHeroInAnyParty(heroInstanceId, out PlayerState owner) is CardInstance hero)
            {
                if (owner.isProtected) return;
                owner.party.Remove(hero);
                thief.party.Add(hero);
            }
        }

        void DestroyHero(string heroInstanceId, GameState state)
        {
            if (state.FindHeroInAnyParty(heroInstanceId, out PlayerState owner) is CardInstance hero)
            {
                if (owner.isProtected) return;
                owner.party.Remove(hero);
                state.discardPile.Add(hero);
            }
        }

        void ReturnHeroToHand(string heroInstanceId, GameState state)
        {
            if (state.FindHeroInAnyParty(heroInstanceId, out PlayerState owner) is CardInstance hero)
            {
                owner.party.Remove(hero);
                owner.hand.Add(hero);
            }
        }

        void PullHeroFromDiscard(string heroInstanceId, PlayerState actor, GameState state)
        {
            for (int i = 0; i < state.discardPile.Count; i++)
            {
                if (state.discardPile[i].instanceId == heroInstanceId &&
                    state.discardPile[i].cardType   == CardType.Hero)
                {
                    var hero = state.discardPile[i];
                    state.discardPile.RemoveAt(i);
                    actor.party.Add(hero);
                    return;
                }
            }
        }

        void SwapHeroes(PlayerState actor, string targetPlayerId,
                        string theirHeroId, string yourHeroId, GameState state)
        {
            var target = state.GetPlayer(targetPlayerId);
            if (target == null || target.isProtected) return;

            CardInstance yourHero = null, theirHero = null;
            foreach (var c in actor.party)  if (c.instanceId == yourHeroId)  { yourHero  = c; break; }
            foreach (var c in target.party) if (c.instanceId == theirHeroId) { theirHero = c; break; }

            if (yourHero == null || theirHero == null) return;
            actor.party.Remove(yourHero);
            target.party.Remove(theirHero);
            actor.party.Add(theirHero);
            target.party.Add(yourHero);
        }

        void StealMagicItem(PlayerState thief, string itemInstanceId, GameState state)
        {
            foreach (var p in state.players)
            {
                for (int i = 0; i < p.magicItems.Count; i++)
                {
                    if (p.magicItems[i].instanceId == itemInstanceId)
                    {
                        var item = p.magicItems[i];
                        p.magicItems.RemoveAt(i);
                        thief.magicItems.Add(item);
                        return;
                    }
                }
            }
        }

        void DestroyMagicItem(string itemInstanceId, GameState state)
        {
            foreach (var p in state.players)
            {
                for (int i = 0; i < p.magicItems.Count; i++)
                {
                    if (p.magicItems[i].instanceId == itemInstanceId)
                    {
                        state.discardPile.Add(p.magicItems[i]);
                        p.magicItems.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        // ────────────────────────────────────────────────────────────────────

        PlayerState ResolveTargetPlayer(EffectTarget target, PlayerState actor,
                                        GameAction action, GameState state)
        {
            return target switch
            {
                EffectTarget.Self        => actor,
                EffectTarget.AnyPlayer   => state.GetPlayer(action.targetPlayerId),
                EffectTarget.AllPlayers  => null, // handled per-effect above
                EffectTarget.LeftPlayer  => GetAdjacentPlayer(actor, state, -1),
                EffectTarget.RightPlayer => GetAdjacentPlayer(actor, state, +1),
                _                        => null
            };
        }

        PlayerState GetAdjacentPlayer(PlayerState actor, GameState state, int direction)
        {
            int count = state.players.Count;
            int idx   = ((actor.playerIndex + direction) % count + count) % count;
            return state.players[idx];
        }
    }
}
