using UnityEngine;

[System.Serializable]
public class PerkData
{
    public PerkType type;
    public PerkRarity rarity;

    public string displayName;
    public string description;

    public float value;

    public PerkData(
        PerkType type,
        PerkRarity rarity,
        string displayName,
        string description,
        float value)
    {
        this.type = type;
        this.rarity = rarity;
        this.displayName = displayName;
        this.description = description;
        this.value = value;
    }
}