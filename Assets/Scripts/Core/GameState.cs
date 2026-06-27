using System;
using System.Collections.Generic;
using UnityEngine;

namespace HereToSlay.Core
{
    public enum GamePhase
    {
        Lobby,
        Setup,
        PlayerTurn,
        MonsterChallenge,   // active player declared a slay attempt
        ReactionWindow,     // other players may play modifier cards
        ResolveDice,
        ApplyEffect,
        CheckWin,
        GameOver
    }

    public enum ActionType
    {
        PlayCard,
        UseHeroAbility,
        ChallengeMonster,
        DrawCard,
        EndTurn,
        // Reactions (used out-of-turn)
        PlayModifier,
        PassReaction,
    }

    [Serializable]
    public class GameAction
    {
        public ActionType   actionType;
        public string       actingPlayerId;
        public string       cardInstanceId;     // card being played / used
        public string       targetInstanceId;   // target card (hero, monster, item)
        public string       targetPlayerId;     // target player (for steal etc.)
    }

    [Serializable]
    public class DiceRollResult
    {
        public int          rawRoll;
        public int          totalModifier;
        public int          finalRoll;
        public bool         success;
        public int          requiredRoll;
        public List<string> appliedModifierIds = new();
    }

    [Serializable]
    public class GameState
    {
        // ── Players ──────────────────────────────────────────────────────────
        public List<PlayerState>   players         = new();
        public int                 activePlayerIndex;
        public GamePhase           phase           = GamePhase.Lobby;

        // ── Decks ────────────────────────────────────────────────────────────
        public List<CardInstance>  mainDeck        = new();
        public List<CardInstance>  discardPile     = new();
        public List<CardInstance>  monsterRow      = new();    // 3 face-up monsters
        public List<CardInstance>  monsterDeck     = new();

        // ── Turn tracking ────────────────────────────────────────────────────
        public int                 actionsRemaining;
        public const int           ActionsPerTurn  = 3;

        // ── Active challenge ─────────────────────────────────────────────────
        public string              challengePlayerId;
        public string              challengeMonsterInstanceId;
        public DiceRollResult      pendingRoll;
        public List<string>        playersYetToReact  = new();

        // ── Win conditions ───────────────────────────────────────────────────
        public int                 heroClassesRequired = 6;    // 6 base, 10 with all expansions
        public int                 monstersToWin       = 3;

        // ── Config ───────────────────────────────────────────────────────────
        public bool                useExpansions;
        public string              winnerPlayerId;

        // ── Helpers ──────────────────────────────────────────────────────────

        public PlayerState GetPlayer(string id)
        {
            foreach (var p in players)
                if (p.playerId == id) return p;
            return null;
        }

        public PlayerState GetActivePlayer() => players[activePlayerIndex];

        public PlayerState GetNextPlayer()
        {
            int next = (activePlayerIndex + 1) % players.Count;
            return players[next];
        }

        public bool IsActiveTurn(string playerId) =>
            players[activePlayerIndex].playerId == playerId &&
            phase == GamePhase.PlayerTurn;

        public bool CanReact(string playerId) =>
            phase == GamePhase.ReactionWindow &&
            playersYetToReact.Contains(playerId);

        public CardInstance FindCardInMonsterRow(string instanceId)
        {
            foreach (var c in monsterRow)
                if (c.instanceId == instanceId) return c;
            return null;
        }

        public CardInstance FindCardInAnyHand(string instanceId, out PlayerState owner)
        {
            foreach (var p in players)
                foreach (var c in p.hand)
                    if (c.instanceId == instanceId) { owner = p; return c; }
            owner = null;
            return null;
        }

        public CardInstance FindHeroInAnyParty(string instanceId, out PlayerState owner)
        {
            foreach (var p in players)
                foreach (var c in p.party)
                    if (c.instanceId == instanceId) { owner = p; return c; }
            owner = null;
            return null;
        }

        public bool CheckWinCondition(out string winnerId)
        {
            foreach (var p in players)
            {
                if (p.UniqueHeroClassCount() >= heroClassesRequired)
                { winnerId = p.playerId; return true; }

                if (p.monstersSlain >= monstersToWin)
                { winnerId = p.playerId; return true; }
            }
            winnerId = null;
            return false;
        }

        public void AdvanceTurn()
        {
            activePlayerIndex = (activePlayerIndex + 1) % players.Count;
            actionsRemaining  = ActionsPerTurn;
            phase             = GamePhase.PlayerTurn;
        }

        public void ReplenishMonsterRow(List<CardInstance> refillSource)
        {
            while (monsterRow.Count < 3 && refillSource.Count > 0)
            {
                monsterRow.Add(refillSource[0]);
                refillSource.RemoveAt(0);
            }
        }
    }
}
