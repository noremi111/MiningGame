using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Energy")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy = 100f;

    [Header("Mining")]
    [SerializeField] private int miningDamage = 1;
    [SerializeField] private float energyCostPerHit = 1f;

    [Header("Luck")]
    [SerializeField] private float luck = 0f;

    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy + buildingMaxEnergyBonus;
    private float buildingMaxEnergyBonus;

    public int MiningDamage => miningDamage;
    public float EnergyCostPerHit => energyCostPerHit;

    public float Luck => luck;

    private void Awake()
    {
        ApplyPermanentUpgrades();

        SyncBuildingEnergyBonus();
        currentEnergy = MaxEnergy;
    }

    private void Update()
    {
        SyncBuildingEnergyBonus();
    }

    // Separate contribution: permanent upgrades and run perks keep their own base value.
    private void SyncBuildingEnergyBonus()
    {
        float next = BuildingEffectManager.Instance != null
            ? BuildingEffectManager.Instance.GetMaxEnergyBonus() : 0f;
        float delta = next - buildingMaxEnergyBonus;
        if (Mathf.Approximately(delta, 0f)) return;
        buildingMaxEnergyBonus = next;
        // Newly added capacity is filled once; ordinary refreshes do not heal.
        currentEnergy = Mathf.Clamp(currentEnergy + Mathf.Max(0f, delta), 0f, MaxEnergy);
    }

    // =========================================================
    // PERMANENT UPGRADES
    // =========================================================

    private void ApplyPermanentUpgrades()
    {
        if (PermanentUpgradeManager.Instance == null)
            return;

        PermanentUpgradeManager upgrades =
            PermanentUpgradeManager.Instance;

        // Pickaxe Upgrade
        miningDamage +=
            upgrades.GetMiningDamageBonus();

        // Energy Upgrade
        maxEnergy +=
            upgrades.GetMaxEnergyBonus();

        // Luck Upgrade
        luck +=
            upgrades.GetLuckBonus();
    }

    // =========================================================
    // ENERGY
    // =========================================================

    public bool CanSpendEnergy(float amount)
    {
        return currentEnergy >= amount;
    }

    public void SpendEnergy(float amount)
    {
        currentEnergy -= amount;

        currentEnergy =
            Mathf.Max(
                0f,
                currentEnergy
            );
    }

    public void RestoreEnergy(float amount)
    {
        SyncBuildingEnergyBonus();
        currentEnergy += amount;

        currentEnergy =
            Mathf.Min(
                currentEnergy,
                MaxEnergy
            );
    }

    public void RestoreFullEnergy()
    {
        SyncBuildingEnergyBonus();
        currentEnergy = MaxEnergy;
    }

    // =========================================================
    // RUN PERKS
    // =========================================================

    public void AddMiningDamage(int amount)
    {
        miningDamage += amount;
    }

    public void ReduceEnergyCost(float percent)
    {
        energyCostPerHit *=
            1f - percent;

        energyCostPerHit =
            Mathf.Max(
                0.1f,
                energyCostPerHit
            );
    }

    public void AddMaxEnergy(float amount)
    {
        maxEnergy += amount;

        currentEnergy += amount;

        currentEnergy =
            Mathf.Min(
                currentEnergy,
                MaxEnergy
            );
    }

    public void AddLuck(float amount)
    {
        luck += amount;
    }
}