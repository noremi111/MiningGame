using System.Collections.Generic;
using UnityEngine;

// Created on the persistent RunManager, not on Surface building objects.
public class BuildingEffectManager : MonoBehaviour
{
    public static BuildingEffectManager Instance { get; private set; }
    private readonly HashSet<int> visitedDepths = new HashSet<int>();
    private readonly List<BuildingEffect> activeEffects = new List<BuildingEffect>();
    private BuildingProgressManager progress;
    private float maxEnergyBonus;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Update() { BindProgress(); }

    private void BindProgress()
    {
        var current = BuildingProgressManager.Instance;
        if (ReferenceEquals(current, progress)) return;
        if (progress != null) progress.ProgressChanged -= RefreshEffects;
        progress = current;
        if (progress != null) progress.ProgressChanged += RefreshEffects;
        RefreshEffects();
    }

    private void RefreshEffects()
    {
        activeEffects.Clear();
        maxEnergyBonus = 0f;
        if (progress == null) return;
        var seenBuildings = new HashSet<string>();
        foreach (var definition in progress.GetBuildingDefinitions())
        {
            if (definition == null || !progress.IsUnlocked(definition) ||
                !seenBuildings.Add(definition.buildingId) || definition.effects == null) continue;
            var seenEffects = new HashSet<BuildingEffect>();
            foreach (var effect in definition.effects)
            {
                if (effect == null || !seenEffects.Add(effect)) continue;
                activeEffects.Add(effect);
                maxEnergyBonus += effect.MaxEnergyBonus;
            }
        }
    }

    public float GetMaxEnergyBonus()
    {
        BindProgress();
        return maxEnergyBonus;
    }

    public void BeginRun()
    {
        visitedDepths.Clear();
        BindProgress();
        RefreshEffects();
    }

    // Called only at the successful end of GenerateMine. First floor is a baseline.
    public void OnFloorReady(int depth, PlayerStats stats)
    {
        if (RunManager.Instance == null || !RunManager.Instance.RunActive) return;
        bool firstFloor = visitedDepths.Count == 0;
        if (!visitedDepths.Add(depth) || firstFloor) return;
        BindProgress();
        RefreshEffects();
        foreach (var effect in activeEffects) effect.OnNewDepth(stats);
    }

    private void OnDestroy()
    {
        if (progress != null) progress.ProgressChanged -= RefreshEffects;
        if (Instance == this) Instance = null;
    }
}
