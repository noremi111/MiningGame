using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PerkGenerator perkGenerator;

    [Header("Panel")]
    [SerializeField] private GameObject perkPanel;

    [Header("Perk Cards")]
    [SerializeField] private Button[] perkButtons;
    [SerializeField] private TMP_Text[] perkTexts;
    [SerializeField] private Outline[] perkOutlines;

    private PerkData[] currentPerks;
    public bool IsOpen { get; private set; }
    
    private void Start()
    {
        perkPanel.SetActive(false);

        for (int i = 0; i < perkButtons.Length; i++)
        {
            int index = i;

            perkButtons[i].onClick.AddListener(
                () => SelectPerk(index)
            );
        }
    }

    public void ShowPerks()
    {
        currentPerks =
            perkGenerator.GenerateRandomPerks(
                3,
                playerStats.Luck
            );

        for (int i = 0; i < currentPerks.Length; i++)
        {
            UpdateCard(i);
        }

        IsOpen = true;

        perkPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void UpdateCard(int index)
    {
        PerkData perk = currentPerks[index];

        Color rarityColor =
            GetRarityColor(perk.rarity);

        string rarityHex =
            ColorUtility.ToHtmlStringRGB(
                rarityColor
            );

        // Haupttext bleibt weiß.
        // Nur die Seltenheit bekommt ihre Farbe.
        perkTexts[index].color = Color.white;

        perkTexts[index].text =
            "<b>" +
            perk.displayName +
            "</b>" +
            "\n\n" +
            perk.description +
            "\n\n" +
            "<color=#" +
            rarityHex +
            "><b>" +
            GetRarityName(perk.rarity) +
            "</b></color>";

        // Kartenrand färben
        if (index < perkOutlines.Length &&
            perkOutlines[index] != null)
        {
            perkOutlines[index].effectColor =
                rarityColor;
        }
    }

    private void SelectPerk(int index)
    {
        if (!IsOpen || currentPerks == null)
            return;

        if (index < 0 ||
            index >= currentPerks.Length)
            return;

        // Lock before applying the perk to ignore repeated button callbacks.
        IsOpen = false;
        ApplyPerk(currentPerks[index]);
        MiningAudio.Current?.Play(MiningAudio.Cue.PerkSelect);
        currentPerks = null;

        perkPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void ApplyPerk(PerkData perk)
    {
        switch (perk.type)
        {
            case PerkType.MiningDamage:

                playerStats.AddMiningDamage(
                    Mathf.RoundToInt(perk.value)
                );

                break;

            case PerkType.EnergyEfficiency:

                playerStats.ReduceEnergyCost(
                    perk.value
                );

                break;

            case PerkType.MaxEnergy:

                playerStats.AddMaxEnergy(
                    perk.value
                );

                break;

            case PerkType.MoveSpeed:

                playerMovement.AddMoveSpeed(
                    perk.value
                );

                break;
            case PerkType.Luck:

                playerStats.AddLuck(
                    perk.value
                );

                break;
        }
    }

    private string GetRarityName(
        PerkRarity rarity)
    {
        return rarity switch
        {
            PerkRarity.Common =>
                "COMMON",

            PerkRarity.Uncommon =>
                "UNCOMMON",

            PerkRarity.Rare =>
                "RARE",

            PerkRarity.Epic =>
                "EPIC",

            PerkRarity.Legendary =>
                "LEGENDARY",

            _ => ""
        };
    }

    private Color GetRarityColor(
        PerkRarity rarity)
    {
        return rarity switch
        {
            PerkRarity.Common =>
                new Color(0.9f, 0.9f, 0.9f),

            PerkRarity.Uncommon =>
                new Color(0.25f, 0.9f, 0.3f),

            PerkRarity.Rare =>
                new Color(0.25f, 0.5f, 1f),

            PerkRarity.Epic =>
                new Color(0.7f, 0.25f, 1f),

            PerkRarity.Legendary =>
                new Color(1f, 0.65f, 0.05f),

            _ =>
                Color.white
        };
    }
    public void ForceClose()
    {
        IsOpen = false;

        if (perkPanel != null)
            perkPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}