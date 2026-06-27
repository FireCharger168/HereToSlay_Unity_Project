using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HereToSlay.Core;

namespace HereToSlay.UI
{
    /// <summary>
    /// Compact panel showing an opponent's party, item count, and score.
    /// Populate() is called by UIManager every state update.
    /// </summary>
    public class OpponentPanel : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI playerNameLabel;
        [SerializeField] TextMeshProUGUI monstersSlainLabel;
        [SerializeField] TextMeshProUGUI heroCountLabel;
        [SerializeField] TextMeshProUGUI itemCountLabel;
        [SerializeField] TextMeshProUGUI handCountLabel;
        [SerializeField] Transform       partyHeroArea;
        [SerializeField] GameObject      miniHeroPrefab;   // tiny card showing class icon

        public void Populate(PlayerState player, GameState state)
        {
            if (playerNameLabel)    playerNameLabel.text    = player.playerName;
            if (monstersSlainLabel) monstersSlainLabel.text = $"☠ {player.monstersSlain}/{state.monstersToWin}";
            if (heroCountLabel)     heroCountLabel.text     = $"🗡 {player.UniqueHeroClassCount()}/{state.heroClassesRequired}";
            if (itemCountLabel)     itemCountLabel.text     = $"🔮 {player.magicItems.Count}";
            if (handCountLabel)     handCountLabel.text     = $"✋ {player.hand.Count}";

            // Mini party display
            if (partyHeroArea != null && miniHeroPrefab != null)
            {
                for (int i = partyHeroArea.childCount - 1; i >= 0; i--)
                    Destroy(partyHeroArea.GetChild(i).gameObject);

                foreach (var hero in player.party)
                {
                    var go  = Instantiate(miniHeroPrefab, partyHeroArea);
                    var img = go.GetComponentInChildren<Image>();
                    var lbl = go.GetComponentInChildren<TextMeshProUGUI>();
                    if (lbl && hero.heroData != null)
                        lbl.text = hero.heroData.heroClass.ToString()[..2].ToUpper();
                }
            }
        }
    }
}
