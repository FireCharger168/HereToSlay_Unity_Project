using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HereToSlay.Core;

namespace HereToSlay.UI
{
    /// <summary>
    /// Attached to each card GameObject in the UI.
    /// Reads from CardInstance and populates all visuals.
    /// </summary>
    public class CardUIItem : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Visual Elements")]
        [SerializeField] Image            cardArtwork;
        [SerializeField] Image            cardFrame;       // colour-tinted by class
        [SerializeField] TextMeshProUGUI  cardNameText;
        [SerializeField] TextMeshProUGUI  cardTypeText;
        [SerializeField] TextMeshProUGUI  descriptionText;
        [SerializeField] TextMeshProUGUI  rollText;        // "Roll 4+" or "Slay: 7+"
        [SerializeField] GameObject       highlightBorder;
        [SerializeField] Button           clickButton;

        [Header("Class Colours")]
        [SerializeField] Color fighterColour     = new(0.8f, 0.2f, 0.2f);
        [SerializeField] Color bardColour        = new(0.8f, 0.6f, 0.1f);
        [SerializeField] Color wizardColour      = new(0.3f, 0.2f, 0.8f);
        [SerializeField] Color guardianColour    = new(0.2f, 0.6f, 0.2f);
        [SerializeField] Color rangerColour      = new(0.5f, 0.8f, 0.2f);
        [SerializeField] Color thiefColour       = new(0.5f, 0.1f, 0.6f);
        [SerializeField] Color warriorColour     = new(0.7f, 0.3f, 0.1f);
        [SerializeField] Color druidColour       = new(0.1f, 0.5f, 0.3f);
        [SerializeField] Color necromancerColour = new(0.2f, 0.1f, 0.3f);
        [SerializeField] Color paladinColour     = new(0.9f, 0.85f, 0.2f);
        [SerializeField] Color monsterColour     = new(0.3f, 0.3f, 0.3f);
        [SerializeField] Color itemColour        = new(0.1f, 0.5f, 0.7f);
        [SerializeField] Color modifierColour    = new(0.6f, 0.6f, 0.6f);

        // ── Data ──────────────────────────────────────────────────────────────
        public CardInstance Card      { get; private set; }
        public CardZone     Zone      { get; private set; }

        public event Action<CardUIItem> OnClicked;

        // ════════════════════════════════════════════════════════════════════

        void Awake()
        {
            clickButton?.onClick.AddListener(() => OnClicked?.Invoke(this));
            highlightBorder?.SetActive(false);
        }

        public void Populate(CardInstance card, CardZone zone)
        {
            Card = card;
            Zone = zone;

            switch (card.cardType)
            {
                case CardType.Hero:        PopulateHero(card);       break;
                case CardType.Monster:     PopulateMonster(card);    break;
                case CardType.MagicItem:   PopulateMagicItem(card);  break;
                case CardType.Modifier:    PopulateModifier(card);   break;
                case CardType.PartyLeader: PopulateLeader(card);     break;
            }

            // Artwork: use ScriptableObject sprite if available, else SVG fallback
            Sprite sprite = GetArtworkSprite(card);
            if (cardArtwork != null && sprite != null)
                cardArtwork.sprite = sprite;
        }

        // ── Per-type populators ───────────────────────────────────────────────

        void PopulateHero(CardInstance card)
        {
            var d = card.heroData;
            if (d == null) { SetTexts(card.dataAssetName, "Hero", "", ""); return; }

            string rollStr = d.rollRequirement > 0 ? $"Roll {d.rollRequirement}+" : "";
            string desc    = d.effects.Count > 0 ? d.effects[0].description : "";

            SetTexts(d.cardName, $"{d.heroClass} Hero", rollStr, desc);
            SetFrameColour(ClassColour(d.heroClass));
        }

        void PopulateMonster(CardInstance card)
        {
            var d = card.monsterData;
            if (d == null) { SetTexts(card.dataAssetName, "Monster", "", ""); return; }

            string reward = d.rewardEffects.Count > 0 ? d.rewardEffects[0].description : "";
            SetTexts(d.cardName, "Monster", $"Slay: {d.rollToSlay}+", $"Reward: {reward}");
            SetFrameColour(monsterColour);
        }

        void PopulateMagicItem(CardInstance card)
        {
            var d = card.magicItemData;
            if (d == null) { SetTexts(card.dataAssetName, "Magic Item", "", ""); return; }

            string typeTag = d.isPersistent ? "[Persistent]" : "[One-Time]";
            string desc    = d.effects.Count > 0 ? $"{typeTag} {d.effects[0].description}" : typeTag;
            SetTexts(d.cardName, "Magic Item", "", desc);
            SetFrameColour(itemColour);
        }

        void PopulateModifier(CardInstance card)
        {
            var d = card.modifierData;
            if (d == null) { SetTexts(card.dataAssetName, "Modifier", "", ""); return; }

            string mod = d.isForceReroll ? "Force Reroll"
                       : d.rollModifier >= 0 ? $"+{d.rollModifier} to Roll"
                       : $"{d.rollModifier} to Roll";
            SetTexts(d.cardName, "Modifier", mod, "Play during any dice roll.");
            SetFrameColour(modifierColour);
        }

        void PopulateLeader(CardInstance card)
        {
            var d = card.leaderData;
            if (d == null) { SetTexts(card.dataAssetName, "Party Leader", "", ""); return; }

            string desc = d.passiveEffects.Count > 0 ? d.passiveEffects[0].description : "";
            SetTexts(d.cardName, $"{d.leaderClass} Leader", "[Passive]", desc);
            SetFrameColour(ClassColour(d.leaderClass));
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        void SetTexts(string name, string type, string roll, string desc)
        {
            if (cardNameText)    cardNameText.text    = name;
            if (cardTypeText)    cardTypeText.text    = type;
            if (rollText)        rollText.text        = roll;
            if (descriptionText) descriptionText.text = desc;
        }

        void SetFrameColour(Color c)
        {
            if (cardFrame) cardFrame.color = c;
        }

        public void SetHighlight(bool on) => highlightBorder?.SetActive(on);

        Sprite GetArtworkSprite(CardInstance card)
        {
            // Try to load from Resources/CardArt/{assetName}
            return Resources.Load<Sprite>($"CardArt/{card.dataAssetName}");
        }

        Color ClassColour(HeroClass cls) => cls switch
        {
            HeroClass.Fighter      => fighterColour,
            HeroClass.Bard         => bardColour,
            HeroClass.Wizard       => wizardColour,
            HeroClass.Guardian     => guardianColour,
            HeroClass.Ranger       => rangerColour,
            HeroClass.Thief        => thiefColour,
            HeroClass.Warrior      => warriorColour,
            HeroClass.Druid        => druidColour,
            HeroClass.Necromancer  => necromancerColour,
            HeroClass.Paladin      => paladinColour,
            _ => Color.white
        };
    }
}
