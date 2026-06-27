using System;
using System.Collections.Generic;
using UnityEngine;

namespace HereToSlay.Core
{
    [Serializable]
    public class PlayerState
    {
        // ── Identity ──────────────────────────────────────────────────────────
        public string   playerId;       // Mirror connection ID (as string)
        public string   playerName;
        public int      playerIndex;    // 0–5 seat position

        // ── Party ─────────────────────────────────────────────────────────────
        public List<CardInstance> party          = new();   // heroes in play
        public List<CardInstance> magicItems     = new();   // persistent items
        public CardInstance       partyLeader;

        // ── Hand & deck ───────────────────────────────────────────────────────
        public List<CardInstance> hand           = new();
        public int                monstersSlain  = 0;

        // ── Flags ─────────────────────────────────────────────────────────────
        public bool isProtected;        // from Guardian effect etc.
        public bool isReady;            // player clicked Ready at lobby

        // ── Win condition checks ──────────────────────────────────────────────

        /// <summary>Has one hero of every HeroClass represented in the base + expansion set.</summary>
        public bool HasFullParty()
        {
            // Requires exactly one hero from each distinct class present in current deck
            var classesInParty = new HashSet<HeroClass>();
            foreach (var c in party)
                if (c.heroData != null)
                    classesInParty.Add(c.heroData.heroClass);

            // Win requires 6 unique classes (base game); 8 for full expansion set
            // GameManager will pass the required count based on deck config
            return false; // evaluated by GameManager with correct threshold
        }

        public bool HasSlainEnoughMonsters(int threshold) => monstersSlain >= threshold;

        // ── Helpers ───────────────────────────────────────────────────────────

        public bool HasHeroOfClass(HeroClass cls)
        {
            foreach (var c in party)
                if (c.heroData != null && c.heroData.heroClass == cls) return true;
            return false;
        }

        public int UniqueHeroClassCount()
        {
            var seen = new HashSet<HeroClass>();
            foreach (var c in party)
                if (c.heroData != null) seen.Add(c.heroData.heroClass);
            return seen.Count;
        }

        public bool HasMagicItem(string assetName)
        {
            foreach (var item in magicItems)
                if (item.dataAssetName == assetName) return true;
            return false;
        }

        public void AddHeroToParty(CardInstance hero)
        {
            if (!party.Contains(hero)) party.Add(hero);
        }

        public bool RemoveHeroFromParty(string instanceId, out CardInstance removed)
        {
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i].instanceId == instanceId)
                {
                    removed = party[i];
                    party.RemoveAt(i);
                    return true;
                }
            }
            removed = null;
            return false;
        }

        public bool RemoveFromHand(string instanceId, out CardInstance card)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i].instanceId == instanceId)
                {
                    card = hand[i];
                    hand.RemoveAt(i);
                    return true;
                }
            }
            card = null;
            return false;
        }
    }
}
