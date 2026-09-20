#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class BuildingEnergyEffectSetup
{
    [MenuItem("Tools/MiningGame/Setup Hospital And Inn Effects")]
    public static void Setup()
    {
        const string folder = "Assets/BuildingEffects";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "BuildingEffects");
        var hospital = GetOrCreate<NewDepthEnergyBuildingEffect>(folder + "/HospitalEnergy.asset");
        var inn = GetOrCreate<MaxEnergyBuildingEffect>(folder + "/InnMaxEnergy.asset");
        int linked = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:BuildingDefinition"))
        {
            var definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            BuildingEffect effect = definition.buildingId == "hospital" ? (BuildingEffect)hospital :
                definition.buildingId == "inn" ? inn : null;
            if (effect == null) continue;
            var list = new System.Collections.Generic.List<BuildingEffect>(definition.effects ?? new BuildingEffect[0]);
            // Preserve customized effects of the same type; avoid adding a second energy bonus.
            if (!list.Exists(e => e != null && e.GetType() == effect.GetType()))
            {
                Undo.RecordObject(definition, "Assign building energy effect");
                list.Add(effect);
                definition.effects = list.ToArray();
                EditorUtility.SetDirty(definition);
            }
            linked++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Energy effect assets ready. Matching hospital/inn definitions: " + linked +
            ". Check Effects on both definitions; assign manually for custom IDs.");
    }

    private static T GetOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}
#endif
