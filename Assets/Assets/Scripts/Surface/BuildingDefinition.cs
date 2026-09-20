using System;
using UnityEngine;

public enum BuildingRequirementType
{
    SilverBlocks, CompletedRuns, SmithHammer, StoneBlocks,
    CompletedGuildQuests, SpecificArtifact, UniqueArtifacts
}

[Serializable]
public class BuildingRequirement
{
    public BuildingRequirementType type;
    [Min(1)] public int amount = 1;
    public ArtifactType artifact;
}

[CreateAssetMenu(menuName = "MiningGame/Building Definition")]
public class BuildingDefinition : ScriptableObject
{
    [Tooltip("Unique stable ID. Never share an ID between different buildings.")]
    public string buildingId;
    [Header("Passive building information")]
    [TextArea(3, 10)] public string effectDescription;
    [Header("Unlock notification")]
    public string displayName;
    public Sprite buildingSprite;
    [TextArea] public string unlockDescription;

    public BuildingRequirement[] requirements = new BuildingRequirement[1];
    [Tooltip("Off: all requirements. On: at least one requirement.")]
    public bool anyRequirement;

    [Header("Effects while built")]
    public BuildingEffect[] effects = new BuildingEffect[0];
}
