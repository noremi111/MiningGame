using UnityEngine;

public class PermanentUpgradeManager : MonoBehaviour
{
    public static PermanentUpgradeManager Instance { get; private set; }

    [Header("Upgrade Levels")]
    [SerializeField] private int pickaxeLevel = 0;
    [SerializeField] private int energyLevel = 0;
    [SerializeField] private int luckLevel = 0;
    [SerializeField] private int bootsLevel = 0;

    [Header("Base Costs")]
    [SerializeField] private int pickaxeBaseCost = 100;
    [SerializeField] private int energyBaseCost = 75;
    [SerializeField] private int luckBaseCost = 125;
    [SerializeField] private int bootsBaseCost = 100;

    [Header("Cost Scaling")]
    [SerializeField] private float costMultiplier = 1.75f;

    public int PickaxeLevel => pickaxeLevel;
    public int EnergyLevel => energyLevel;
    public int LuckLevel => luckLevel;
    public int BootsLevel => bootsLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================================================
    // COSTS
    // =========================================================

    public int GetPickaxeCost()
    {
        return CalculateCost(
            pickaxeBaseCost,
            pickaxeLevel
        );
    }

    public int GetEnergyCost()
    {
        return CalculateCost(
            energyBaseCost,
            energyLevel
        );
    }

    public int GetLuckCost()
    {
        return CalculateCost(
            luckBaseCost,
            luckLevel
        );
    }

    public int GetBootsCost()
    {
        return CalculateCost(
            bootsBaseCost,
            bootsLevel
        );
    }

    private int CalculateCost(
        int baseCost,
        int level)
    {
        return Mathf.RoundToInt(
            baseCost *
            Mathf.Pow(
                costMultiplier,
                level
            )
        );
    }

    // =========================================================
    // BUY UPGRADES
    // =========================================================

    public bool BuyPickaxeUpgrade()
    {
        int cost = GetPickaxeCost();

        if (!TrySpendGold(cost))
            return false;

        pickaxeLevel++;

        return true;
    }

    public bool BuyEnergyUpgrade()
    {
        int cost = GetEnergyCost();

        if (!TrySpendGold(cost))
            return false;

        energyLevel++;

        return true;
    }

    public bool BuyLuckUpgrade()
    {
        int cost = GetLuckCost();

        if (!TrySpendGold(cost))
            return false;

        luckLevel++;

        return true;
    }

    public bool BuyBootsUpgrade()
    {
        int cost = GetBootsCost();

        if (!TrySpendGold(cost))
            return false;

        bootsLevel++;

        return true;
    }

    private bool TrySpendGold(int amount)
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError(
                "RunManager wurde nicht gefunden!"
            );

            return false;
        }

        return RunManager.Instance
            .SpendBankGold(amount);
    }

    // =========================================================
    // PERMANENT BONUSES
    // =========================================================

    public int GetMiningDamageBonus()
    {
        return pickaxeLevel;
    }

    public float GetMaxEnergyBonus()
    {
        return energyLevel * 10f;
    }

    public float GetLuckBonus()
    {
        return luckLevel * 2f;
    }

    public float GetMoveSpeedMultiplier()
    {
        return 1f +
               (bootsLevel * 0.05f);
    }
}