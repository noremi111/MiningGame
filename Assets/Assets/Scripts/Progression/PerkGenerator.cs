using System.Collections.Generic;
using UnityEngine;

public class PerkGenerator : MonoBehaviour
{
    public PerkData[] GenerateRandomPerks(
        int amount,
        float luck)
    {
        List<PerkType> availableTypes =
            new List<PerkType>
            {
                PerkType.MiningDamage,
                PerkType.EnergyEfficiency,
                PerkType.MaxEnergy,
                PerkType.MoveSpeed,
                PerkType.Luck
            };

        amount =
            Mathf.Min(amount, availableTypes.Count);

        PerkData[] perks =
            new PerkData[amount];

        for (int i = 0; i < amount; i++)
        {
            int randomIndex =
                Random.Range(
                    0,
                    availableTypes.Count
                );

            PerkType selectedType =
                availableTypes[randomIndex];

            availableTypes.RemoveAt(
                randomIndex
            );

            PerkRarity rarity =
                RollRarity(luck);

            perks[i] =
                CreatePerk(
                    selectedType,
                    rarity
                );
        }

        return perks;
    }

    private PerkRarity RollRarity(float luck)
    {
        float commonWeight = 55f;

        float uncommonWeight =
            25f * (1f + luck * 0.01f);

        float rareWeight =
            12f * (1f + luck * 0.02f);

        float epicWeight =
            6f * (1f + luck * 0.03f);

        float legendaryWeight =
            2f * (1f + luck * 0.04f);

        float totalWeight =
            commonWeight +
            uncommonWeight +
            rareWeight +
            epicWeight +
            legendaryWeight;

        float roll =
            Random.Range(0f, totalWeight);

        if (roll < commonWeight)
            return PerkRarity.Common;

        roll -= commonWeight;

        if (roll < uncommonWeight)
            return PerkRarity.Uncommon;

        roll -= uncommonWeight;

        if (roll < rareWeight)
            return PerkRarity.Rare;

        roll -= rareWeight;

        if (roll < epicWeight)
            return PerkRarity.Epic;

        return PerkRarity.Legendary;
    }

    private PerkData CreatePerk(
        PerkType type,
        PerkRarity rarity)
    {
        switch (type)
        {
            case PerkType.MiningDamage:
                return CreateMiningDamagePerk(rarity);

            case PerkType.EnergyEfficiency:
                return CreateEnergyEfficiencyPerk(rarity);

            case PerkType.MaxEnergy:
                return CreateMaxEnergyPerk(rarity);

            case PerkType.MoveSpeed:
                return CreateMoveSpeedPerk(rarity);

            case PerkType.Luck:
                return CreateLuckPerk(rarity);
        }

        return null;
    }

    private PerkData CreateMiningDamagePerk(
        PerkRarity rarity)
    {
        int value = rarity switch
        {
            PerkRarity.Common => 1,
            PerkRarity.Uncommon => 2,
            PerkRarity.Rare => 3,
            PerkRarity.Epic => 5,
            PerkRarity.Legendary => 8,
            _ => 1
        };

        return new PerkData(
            PerkType.MiningDamage,
            rarity,
            "Power Strike",
            "+" + value + " Mining Damage",
            value
        );
    }

    private PerkData CreateEnergyEfficiencyPerk(
        PerkRarity rarity)
    {
        float value = rarity switch
        {
            PerkRarity.Common => 0.05f,
            PerkRarity.Uncommon => 0.10f,
            PerkRarity.Rare => 0.15f,
            PerkRarity.Epic => 0.20f,
            PerkRarity.Legendary => 0.30f,
            _ => 0.05f
        };

        return new PerkData(
            PerkType.EnergyEfficiency,
            rarity,
            "Efficient Swing",
            "-" +
            Mathf.RoundToInt(value * 100f) +
            "% Energy Cost",
            value
        );
    }

    private PerkData CreateMaxEnergyPerk(
        PerkRarity rarity)
    {
        int value = rarity switch
        {
            PerkRarity.Common => 10,
            PerkRarity.Uncommon => 20,
            PerkRarity.Rare => 35,
            PerkRarity.Epic => 60,
            PerkRarity.Legendary => 100,
            _ => 10
        };

        return new PerkData(
            PerkType.MaxEnergy,
            rarity,
            "Endurance",
            "+" + value + " Max Energy",
            value
        );
    }

    private PerkData CreateMoveSpeedPerk(
        PerkRarity rarity)
    {
        float value = rarity switch
        {
            PerkRarity.Common => 0.25f,
            PerkRarity.Uncommon => 0.50f,
            PerkRarity.Rare => 0.75f,
            PerkRarity.Epic => 1.00f,
            PerkRarity.Legendary => 1.50f,
            _ => 0.25f
        };

        return new PerkData(
            PerkType.MoveSpeed,
            rarity,
            "Fleet Footed",
            "+" +
            value.ToString("0.##") +
            " Movement Speed",
            value
        );
    }

    private PerkData CreateLuckPerk(
        PerkRarity rarity)
    {
        int value = rarity switch
        {
            PerkRarity.Common => 5,
            PerkRarity.Uncommon => 10,
            PerkRarity.Rare => 15,
            PerkRarity.Epic => 25,
            PerkRarity.Legendary => 40,
            _ => 5
        };

        return new PerkData(
            PerkType.Luck,
            rarity,
            "Lucky Miner",
            "+" + value + " Luck",
            value
        );
    }
}