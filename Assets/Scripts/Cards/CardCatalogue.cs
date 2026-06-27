using System.Collections.Generic;
using HereToSlay.Core;

namespace HereToSlay.Cards
{
    /// <summary>
    /// Static catalogue of every card in the game.
    /// GameManager loads these at startup and turns them into CardInstance lists.
    /// Each entry matches a ScriptableObject asset name in Resources/Cards/
    /// </summary>
    public static class CardCatalogue
    {
        // ────────────────────────────────────────────────────────────────────
        //  PARTY LEADERS
        // ────────────────────────────────────────────────────────────────────
        public static readonly List<CardDefinition> PartyLeaders = new()
        {
            new("LeaderFighter",   CardType.PartyLeader, "Ser Popstar",    HeroClass.Fighter,
                "Once per turn, gain +1 to any roll."),
            new("LeaderBard",      CardType.PartyLeader, "Minstrel Rex",   HeroClass.Bard,
                "Draw 1 extra card at the start of your turn."),
            new("LeaderWizard",    CardType.PartyLeader, "Archmagus Zara", HeroClass.Wizard,
                "Once per turn, look at the top card of any deck."),
            new("LeaderGuardian",  CardType.PartyLeader, "Ironwall Brynn", HeroClass.Guardian,
                "Your heroes cannot be stolen on other players' turns."),
            new("LeaderRanger",    CardType.PartyLeader, "Swift Arrow",    HeroClass.Ranger,
                "You may attempt to slay a monster for 1 action instead of 1 action."),
            new("LeaderThief",     CardType.PartyLeader, "Shadow Vesper",  HeroClass.Thief,
                "Once per turn, look at another player's hand."),
            new("LeaderWarrior",   CardType.PartyLeader, "Warlord Krynn",  HeroClass.Warrior,
                "Add +2 to rolls when slaying monsters."),
            new("LeaderDruid",     CardType.PartyLeader, "Eldergrove",     HeroClass.Druid,
                "At the start of your turn, draw 1 card if you have no magic items."),
            new("LeaderNecromancer", CardType.PartyLeader, "Dread Morvaine", HeroClass.Necromancer,
                "Once per game, return a hero from the discard to your hand."),
            new("LeaderPaladin",   CardType.PartyLeader, "Holy Dawnbreaker", HeroClass.Paladin,
                "Your heroes are immune to Modifier cards that reduce rolls."),
        };

        // ────────────────────────────────────────────────────────────────────
        //  HEROES — BASE SET
        // ────────────────────────────────────────────────────────────────────
        public static readonly List<CardDefinition> BaseHeroes = new()
        {
            // FIGHTERS
            new("HeroFighter1", CardType.Hero, "Ching Ching",          HeroClass.Fighter,
                "Roll 4+: Destroy a Magic Item any player controls."),
            new("HeroFighter2", CardType.Hero, "Gorgon Zola",          HeroClass.Fighter,
                "Roll 5+: Steal a Hero from another player's party and add it to yours."),
            new("HeroFighter3", CardType.Hero, "Marrow",               HeroClass.Fighter,
                "Passive: +2 to all monster-slay rolls."),

            // BARDS
            new("HeroBard1", CardType.Hero, "Blobby",                  HeroClass.Bard,
                "Roll 3+: Draw 3 cards."),
            new("HeroBard2", CardType.Hero, "Pan Pan",                 HeroClass.Bard,
                "Roll 4+: Force another player to discard 2 cards."),
            new("HeroBard3", CardType.Hero, "Calming Voice",           HeroClass.Bard,
                "Passive: Once per turn, cancel a Modifier card played against you."),

            // WIZARDS
            new("HeroWizard1", CardType.Hero, "Fluffy",                HeroClass.Wizard,
                "Roll 4+: Look at any player's hand and steal 1 card."),
            new("HeroWizard2", CardType.Hero, "Tim the Enchanter",     HeroClass.Wizard,
                "Roll 3+: Add +3 to the next roll you make this turn."),
            new("HeroWizard3", CardType.Hero, "Mage of the Forgotten", HeroClass.Wizard,
                "Passive: You may use an extra Modifier card during any roll."),

            // GUARDIANS
            new("HeroGuardian1", CardType.Hero, "Hitch",               HeroClass.Guardian,
                "Roll 4+: Protect target hero from the next attack against it."),
            new("HeroGuardian2", CardType.Hero, "Grizz",               HeroClass.Guardian,
                "Passive: Heroes in your party cannot be targeted by Steal effects."),
            new("HeroGuardian3", CardType.Hero, "Lochlan the Shield",  HeroClass.Guardian,
                "Roll 5+: Negate any card played by another player this turn."),

            // RANGERS
            new("HeroRanger1", CardType.Hero, "Aaaa",                  HeroClass.Ranger,
                "Roll 3+: Draw 2 cards, then discard 1."),
            new("HeroRanger2", CardType.Hero, "Dart",                  HeroClass.Ranger,
                "Passive: +1 to all your monster-slay rolls."),
            new("HeroRanger3", CardType.Hero, "Wrenna Greenwood",      HeroClass.Ranger,
                "Roll 4+: Return a Hero from any player's party to their hand."),

            // THIEVES
            new("HeroThief1", CardType.Hero, "Elowen Dusk",            HeroClass.Thief,
                "Roll 4+: Steal a Magic Item from another player."),
            new("HeroThief2", CardType.Hero, "Nipsy",                  HeroClass.Thief,
                "Roll 3+: Look at the top 3 cards of the main deck and rearrange them."),
            new("HeroThief3", CardType.Hero, "Shadow Dancer",          HeroClass.Thief,
                "Passive: Once per turn, swap one card from your hand with the top card of the deck."),
        };

        // ────────────────────────────────────────────────────────────────────
        //  HEROES — WARRIORS & DRUIDS EXPANSION
        // ────────────────────────────────────────────────────────────────────
        public static readonly List<CardDefinition> ExpansionHeroes = new()
        {
            // WARRIORS
            new("HeroWarrior1", CardType.Hero, "Berserker Kord",        HeroClass.Warrior,
                "Roll 4+: Deal damage — force target player to discard their lowest-cost card."),
            new("HeroWarrior2", CardType.Hero, "Iron Maiden",           HeroClass.Warrior,
                "Passive: +3 to slay rolls when you already have 2+ monsters slain."),
            new("HeroWarrior3", CardType.Hero, "Bloodthorn",            HeroClass.Warrior,
                "Roll 5+: Destroy a hero in any player's party."),

            // DRUIDS
            new("HeroDruid1", CardType.Hero, "Bramble",                 HeroClass.Druid,
                "Roll 3+: Draw 2 cards and gain 1 extra action this turn."),
            new("HeroDruid2", CardType.Hero, "Sylvan Oracle",           HeroClass.Druid,
                "Passive: Once per turn, you may reroll any die roll (yours or opponent's) once."),
            new("HeroDruid3", CardType.Hero, "The Verdant",             HeroClass.Druid,
                "Roll 4+: Return a card from the discard pile to your hand."),

            // NECROMANCERS
            new("HeroNecromancer1", CardType.Hero, "Shade",             HeroClass.Necromancer,
                "Roll 4+: Return a hero from any discard pile to your party."),
            new("HeroNecromancer2", CardType.Hero, "Dread Whisper",     HeroClass.Necromancer,
                "Passive: When any hero is destroyed, draw 1 card."),
            new("HeroNecromancer3", CardType.Hero, "Bone Mage",         HeroClass.Necromancer,
                "Roll 5+: Force another player to sacrifice a hero of their choice."),

            // PALADINS
            new("HeroPaladin1", CardType.Hero, "Auric Lightbringer",    HeroClass.Paladin,
                "Roll 4+: Protect all heroes in your party until your next turn."),
            new("HeroPaladin2", CardType.Hero, "Sister Seraphine",      HeroClass.Paladin,
                "Passive: Your party leaders' abilities can be used twice per turn."),
            new("HeroPaladin3", CardType.Hero, "The Devoted",           HeroClass.Paladin,
                "Roll 3+: Negate a Modifier card and send it to the discard."),
        };

        // ────────────────────────────────────────────────────────────────────
        //  MONSTERS — BASE SET
        // ────────────────────────────────────────────────────────────────────
        public static readonly List<CardDefinition> BaseMonsters = new()
        {
            new("MonsterDragon",   CardType.Monster, "The Corrupt Dragon",     rollToSlay: 10,
                reward: "Draw 3 cards. Gain 1 extra action on your next turn."),
            new("MonsterWolf",     CardType.Monster, "Spectral Wolf Pack",      rollToSlay: 7,
                reward: "Draw 2 cards."),
            new("MonsterGolem",    CardType.Monster, "Stone Golem",             rollToSlay: 8,
                reward: "Steal a Magic Item from any player."),
            new("MonsterVampire",  CardType.Monster, "Nosferatusk",             rollToSlay: 9,
                reward: "Look at any player's hand. Steal 1 card."),
            new("MonsterOgre",     CardType.Monster, "Bog Ogre",                rollToSlay: 6,
                reward: "Draw 1 card."),
            new("MonsterWitch",    CardType.Monster, "The Coven Witch",         rollToSlay: 8,
                reward: "Force all other players to discard 1 card."),
            new("MonsterHydra",    CardType.Monster, "Hydra",                   rollToSlay: 9,
                reward: "Add a Hero from the discard to your party."),
            new("MonsterBeholder", CardType.Monster, "The All-Seeing Eye",      rollToSlay: 10,
                reward: "Destroy a hero in any player's party."),
            new("MonsterSkeleton", CardType.Monster, "Skeleton Horde",          rollToSlay: 5,
                reward: "Draw 2 cards."),
            new("MonsterTroll",    CardType.Monster, "Bridge Troll",            rollToSlay: 6,
                reward: "Gain 2 extra actions on your next turn."),
            new("MonsterWyvern",   CardType.Monster, "Venomous Wyvern",         rollToSlay: 8,
                reward: "Steal a Hero from another player's party."),
            new("MonsterLich",     CardType.Monster, "Lich King",               rollToSlay: 10,
                reward: "Draw 4 cards. Return a hero from discard to your party."),
        };

        // ────────────────────────────────────────────────────────────────────
        //  MONSTERS — EXPANSION
        // ────────────────────────────────────────────────────────────────────
        public static readonly List<CardDefinition> ExpansionMonsters = new()
        {
            new("MonsterDemon",    CardType.Monster, "Chaos Demon",             rollToSlay: 9,
                reward: "All players discard their hand and draw 3 cards."),
            new("MonsterPhoenix",  CardType.Monster, "Eternal Phoenix",         rollToSlay: 8,
                reward: "Draw 2 cards. Play an extra Magic Item this turn."),
            new("MonsterKraken",   CardType.Monster, "Deep Kraken",             rollToSlay: 10,
                reward: "Steal 2 Magic Items from any players."),
            new("MonsterBanshee",  CardType.Monster, "Wailing Banshee",         rollToSlay: 7,
                reward: "Force two players to each discard 1 card."),
            new("MonsterChimera",  CardType.Monster, "Chimera",                 rollToSlay: 9,
                reward: "Return any hero in any party to its owner's hand."),
        };

        // ────────────────────────────────────────────────────────────────────
        //  MAGIC ITEMS
        // ────────────────────────────────────────────────────────────────────
        public static readonly List<CardDefinition> MagicItems = new()
        {
            new("ItemShield",      CardType.MagicItem, "Enchanted Shield",
                "Persistent. Your heroes cannot be destroyed until this card is discarded."),
            new("ItemPotion",      CardType.MagicItem, "Speed Potion",
                "One-time. Gain 2 extra actions this turn."),
            new("ItemSword",       CardType.MagicItem, "Vorpal Sword",
                "Persistent. +2 to all monster-slay rolls."),
            new("ItemCloak",       CardType.MagicItem, "Cloak of Shadows",
                "Persistent. Your hand size cannot be reduced by card effects."),
            new("ItemScroll",      CardType.MagicItem, "Scroll of Recall",
                "One-time. Return any card from the discard to your hand."),
            new("ItemAmulet",      CardType.MagicItem, "Luck Amulet",
                "One-time. Reroll any die once (yours or an opponent's)."),
            new("ItemGrimoire",    CardType.MagicItem, "Wizard's Grimoire",
                "Persistent. Draw 1 extra card at the start of your turn."),
            new("ItemBow",         CardType.MagicItem, "Ranger's Longbow",
                "Persistent. +1 to all monster-slay rolls. Stack with other bonuses."),
            new("ItemCrown",       CardType.MagicItem, "Crown of Command",
                "One-time. Take 1 extra turn immediately after this one (max once per game)."),
            new("ItemRing",        CardType.MagicItem, "Ring of Protection",
                "One-time. Negate any card effect targeting you or your party."),
            new("ItemOrb",         CardType.MagicItem, "Scrying Orb",
                "Persistent. At any time, you may look at the top card of any deck."),
            new("ItemDagger",      CardType.MagicItem, "Thief's Dagger",
                "One-time. Steal a Magic Item from any player."),
        };

        // ────────────────────────────────────────────────────────────────────
        //  MODIFIER CARDS
        // ────────────────────────────────────────────────────────────────────
        public static readonly List<CardDefinition> Modifiers = new()
        {
            new("ModPlus2",     CardType.Modifier, "Heroic Surge",      modifier: +2,
                "Play during any roll. Add +2 to the result."),
            new("ModPlus3",     CardType.Modifier, "Battle Cry",        modifier: +3,
                "Play during any roll. Add +3 to the result."),
            new("ModMinus2",    CardType.Modifier, "Cursed Dice",       modifier: -2,
                "Play during an opponent's roll. Subtract 2 from the result."),
            new("ModMinus3",    CardType.Modifier, "Hex of Misfortune", modifier: -3,
                "Play during an opponent's roll. Subtract 3 from the result."),
            new("ModReroll",    CardType.Modifier, "Fickle Fate",       modifier: 0,
                "Play during any roll. Force that roll to be rerolled."),
            new("ModPlus1",     CardType.Modifier, "Lucky Break",       modifier: +1,
                "Play during any roll. Add +1 to the result."),
            new("ModMinus1",    CardType.Modifier, "Ill Omen",          modifier: -1,
                "Play during an opponent's roll. Subtract 1 from the result."),
            new("ModPlus5",     CardType.Modifier, "Divine Inspiration",modifier: +5,
                "Play during your own roll only. Add +5 to the result."),
        };

        // ────────────────────────────────────────────────────────────────────
        //  Counts for deck building
        // ────────────────────────────────────────────────────────────────────
        /// <summary>How many copies of each card go into the main deck.</summary>
        public static int CopiesOf(CardDefinition def) => def.cardType switch
        {
            CardType.Modifier   => 3,
            CardType.MagicItem  => 2,
            CardType.Hero       => 1,
            CardType.Monster    => 1,
            _                   => 1
        };
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Lightweight definition used only at startup to build deck
    // ────────────────────────────────────────────────────────────────────────
    public class CardDefinition
    {
        public string    assetName;
        public CardType  cardType;
        public string    displayName;
        public HeroClass heroClass;     // only for Hero / PartyLeader
        public int       rollToSlay;    // only for Monster
        public int       modifier;      // only for Modifier cards
        public string    description;

        // Hero / PartyLeader constructor
        public CardDefinition(string asset, CardType type, string name, HeroClass cls, string desc)
        {
            assetName = asset; cardType = type; displayName = name;
            heroClass = cls; description = desc;
        }

        // Monster constructor
        public CardDefinition(string asset, CardType type, string name, int rollToSlay, string reward)
        {
            assetName = asset; cardType = type; displayName = name;
            this.rollToSlay = rollToSlay; description = reward;
        }

        // MagicItem constructor
        public CardDefinition(string asset, CardType type, string name, string desc)
        {
            assetName = asset; cardType = type; displayName = name; description = desc;
        }

        // Modifier constructor
        public CardDefinition(string asset, CardType type, string name, int mod, string desc)
        {
            assetName = asset; cardType = type; displayName = name;
            modifier = mod; description = desc;
        }
    }
}
