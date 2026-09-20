#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public static class BuildingDefinitionSetup
{
    [MenuItem("Tools/MiningGame/Create Building Definitions")]
    public static void CreateDefinitions()
    {
        const string folder = "Assets/BuildingDefinitions";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "BuildingDefinitions");
        Create("Krankenhaus", "hospital", BuildingRequirementType.SilverBlocks, 50);
        Create("Gasthaus", "inn", BuildingRequirementType.CompletedRuns, 10);
        Create("Schmiede", "smithy", BuildingRequirementType.SmithHammer, 1);
        Create("Abenteurergilde", "adventurers_guild", BuildingRequirementType.StoneBlocks, 100);
        Create("Kaserne", "barracks", BuildingRequirementType.CompletedGuildQuests, 1);
        ArtifactType eye = ArtifactType.None;
        foreach (ArtifactType type in Enum.GetValues(typeof(ArtifactType)))
        {
            string name = type.ToString().ToLowerInvariant();
            if (name.Contains("cartograph") && name.Contains("eye")) { eye = type; break; }
        }
        Create("Kartografenhaus", "cartographer", BuildingRequirementType.SpecificArtifact, 1, eye);
        if (eye == ArtifactType.None)
            Debug.LogWarning("Select Cartographer's Eye manually in Kartografenhaus / Requirements / Artifact.");
        Create("Museum", "museum", BuildingRequirementType.UniqueArtifacts, 3);
        AssetDatabase.SaveAssets();
        Debug.Log("Building definitions ready in Assets/BuildingDefinitions. Existing assets were preserved.");
    }

    private static void Create(string name, string id, BuildingRequirementType type, int amount,
        ArtifactType artifact = ArtifactType.None)
    {
        string path = "Assets/BuildingDefinitions/" + name + ".asset";
        if (AssetDatabase.LoadMainAssetAtPath(path) != null) return;
        var definition = ScriptableObject.CreateInstance<BuildingDefinition>();
        definition.buildingId = id;
        definition.requirements = new[] { new BuildingRequirement { type = type, amount = amount, artifact = artifact } };
        AssetDatabase.CreateAsset(definition, path);
    }
}
#endif
