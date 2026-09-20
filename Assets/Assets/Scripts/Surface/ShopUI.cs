using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text bankGoldText;

    [SerializeField] private TMP_Text pickaxeText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text luckText;
    [SerializeField] private TMP_Text bootsText;

    [Header("Buttons")]
    [SerializeField] private Button pickaxeButton;
    [SerializeField] private Button energyButton;
    [SerializeField] private Button luckButton;
    [SerializeField] private Button bootsButton;

    private void Start()
    {
        pickaxeButton.onClick.AddListener(
            BuyPickaxe
        );

        energyButton.onClick.AddListener(
            BuyEnergy
        );

        luckButton.onClick.AddListener(
            BuyLuck
        );

        bootsButton.onClick.AddListener(
            BuyBoots
        );

        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void BuyPickaxe()
    {
        if (PermanentUpgradeManager.Instance == null)
            return;

        PermanentUpgradeManager.Instance
            .BuyPickaxeUpgrade();

        UpdateUI();
    }

    private void BuyEnergy()
    {
        if (PermanentUpgradeManager.Instance == null)
            return;

        PermanentUpgradeManager.Instance
            .BuyEnergyUpgrade();

        UpdateUI();
    }

    private void BuyLuck()
    {
        if (PermanentUpgradeManager.Instance == null)
            return;

        PermanentUpgradeManager.Instance
            .BuyLuckUpgrade();

        UpdateUI();
    }

    private void BuyBoots()
    {
        if (PermanentUpgradeManager.Instance == null)
            return;

        PermanentUpgradeManager.Instance
            .BuyBootsUpgrade();

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (RunManager.Instance != null &&
            bankGoldText != null)
        {
            bankGoldText.text =
                "Gold: " +
                RunManager.Instance.BankGold;
        }

        PermanentUpgradeManager upgrades =
            PermanentUpgradeManager.Instance;

        if (upgrades == null)
            return;
        if (pickaxeText != null)
        {
            pickaxeText.text =
                "<b>PICKAXE</b>\n\n" +
                "Level " +
                upgrades.PickaxeLevel +
                "\n+1 Mining Damage\n" +
                "Cost: " +
                upgrades.GetPickaxeCost();
        }

        if (energyText != null)
        {
            energyText.text =
                "<b>ENERGY</b>\n\n" +
                "Level " +
                upgrades.EnergyLevel +
                "\n+10 Max Energy\n" +
                "Cost: " +
                upgrades.GetEnergyCost();
        }

        if (luckText != null)
        {
            luckText.text =
                "<b>LUCK</b>\n\n" +
                "Level " +
                upgrades.LuckLevel +
                "\n+2 Starting Luck\n" +
                "Cost: " +
                upgrades.GetLuckCost();
        }

        if (bootsText != null)
        {
            bootsText.text =
                "<b>BOOTS</b>\n\n" +
                "Level " +
                upgrades.BootsLevel +
                "\n+5% Move Speed\n" +
                "Cost: " +
                upgrades.GetBootsCost();
        }

        UpdateButtons(upgrades);
    }

    private void UpdateButtons(
        PermanentUpgradeManager upgrades)
    {
        if (RunManager.Instance == null)
            return;

        int gold =
            RunManager.Instance.BankGold;

        if (pickaxeButton != null)
        {
            pickaxeButton.interactable =
                gold >=
                upgrades.GetPickaxeCost();
        }

        if (energyButton != null)
        {
            energyButton.interactable =
                gold >=
                upgrades.GetEnergyCost();
        }

        if (luckButton != null)
        {
            luckButton.interactable =
                gold >=
                upgrades.GetLuckCost();
        }

        if (bootsButton != null)
        {
            bootsButton.interactable =
                gold >=
                upgrades.GetBootsCost();
        }
    }
}