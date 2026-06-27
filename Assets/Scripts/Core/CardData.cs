using System;
using System.Collections.Generic;
using UnityEngine;

namespace HereToSlay.Core
{
    // ─── Enums ───────────────────────────────────────────────────────────────

    public enum HeroClass
    {
        Fighter, Bard, Wizard, Guardian, Ranger, Thief,
        Warrior, Druid, Necromancer, Paladin  // Expansion classes
    }

    public enum CardType
    {
        PartyLeader, Hero, Monster, MagicItem, Modifier
    }

    public enum EffectTrigger
    {
        OnPlay,         // fires when card enters party
        OnAction,       // activated ability (costs 1 action)
        OnSlayMonster,  // fires when owner slays a monster
        OnDiceRoll,     // can be played during a roll
        Passive,        // always-on while in party
        OnDiscard,      // fires when sent to discard
    }

    public enum EffectTarget
    {
        Self, AnyPlayer, LeftPlayer, RightPlayer,
        AnyHero, OwnHero, EnemyHero,
        AnyMonster, TopDeck, AllPlayers
    }

    public enum EffectType
    {
        DrawCards,
        DiscardCards,
        StealHero,
        DestroyHero,
        ReturnHero,         // bounce hero to hand
        GainActions,
        RollBonus,
        RollPenalty,
        ForceReroll,
        ProtectHero,        // negate next attack on a hero
        SlayMonsterBonus,   // extra reward when slaying
        AddHeroToParty,
        LookAtHand,
        SwapHero,
        HealRoll,           // re-roll and take higher
        CurseRoll,          // re-roll and take lower
        StealMagicItem,
        DestroyMagicItem,
        ExtraMonsterAttempt,
        VictoryPointBonus,
    }

    // ─── Effect definition ────────────────────────────────────────────────────

    [Serializable]
    public class CardEffect
    {
        public EffectType   effectType;
        public EffectTrigger trigger;
        public EffectTarget target;
        public int          value;          // generic int param (cards drawn, roll bonus, etc.)
        public string       description;    // human-readable
    }

    // ─── ScriptableObject card definitions ───────────────────────────────────

    [CreateAssetMenu(menuName = "HereToSlay/HeroCard")]
    public class HeroCardData : ScriptableObject
    {
        public string       cardName;
        [TextArea] public string flavourText;
        public HeroClass    heroClass;
        public Sprite       artwork;        // assign in Inspector; falls back to SVG sprite
        public int          rollRequirement; // minimum d10 roll to use activated ability
        public List<CardEffect> effects = new();
    }

    [CreateAssetMenu(menuName = "HereToSlay/MonsterCard")]
    public class MonsterCardData : ScriptableObject
    {
        public string       cardName;
        [TextArea] public string flavourText;
        public Sprite       artwork;
        public int          rollToSlay;     // d10 target number
        public List<CardEffect> rewardEffects = new();  // granted to slayer
    }

    [CreateAssetMenu(menuName = "HereToSlay/MagicItemCard")]
    public class MagicItemCardData : ScriptableObject
    {
        public string       cardName;
        [TextArea] public string flavourText;
        public Sprite       artwork;
        public bool         isPersistent;   // stays in play vs one-time use
        public List<CardEffect> effects = new();
    }

    [CreateAssetMenu(menuName = "HereToSlay/PartyLeaderCard")]
    public class PartyLeaderCardData : ScriptableObject
    {
        public string       cardName;
        [TextArea] public string flavourText;
        public HeroClass    leaderClass;
        public Sprite       artwork;
        public List<CardEffect> passiveEffects = new();
    }

    [CreateAssetMenu(menuName = "HereToSlay/ModifierCard")]
    public class ModifierCardData : ScriptableObject
    {
        public string       cardName;
        [TextArea] public string flavourText;
        public int          rollModifier;   // positive = bonus, negative = penalty
        public bool         isForceReroll;
        public bool         targetsOwner;   // false = targets opponent
    }

    // ─── Runtime card instance (wraps ScriptableObject) ──────────────────────

    [Serializable]
    public class CardInstance
    {
        public string       instanceId;     // unique GUID
        public CardType     cardType;
        public string       dataAssetName;  // name of the ScriptableObject asset

        // Resolved at runtime (not serialised over network — clients load from asset name)
        [NonSerialized] public HeroCardData      heroData;
        [NonSerialized] public MonsterCardData   monsterData;
        [NonSerialized] public MagicItemCardData magicItemData;
        [NonSerialized] public ModifierCardData  modifierData;
        [NonSerialized] public PartyLeaderCardData leaderData;

        public string DisplayName => cardType switch
        {
            CardType.Hero       => heroData?.cardName       ?? dataAssetName,
            CardType.Monster    => monsterData?.cardName    ?? dataAssetName,
            CardType.MagicItem  => magicItemData?.cardName  ?? dataAssetName,
            CardType.Modifier   => modifierData?.cardName   ?? dataAssetName,
            CardType.PartyLeader=> leaderData?.cardName     ?? dataAssetName,
            _ => dataAssetName
        };

        public static CardInstance Create(CardType type, string assetName)
        {
            return new CardInstance
            {
                instanceId   = Guid.NewGuid().ToString(),
                cardType     = type,
                dataAssetName = assetName
            };
        }
    }
}
