using System;
using System.Collections.Generic;
using UnityEngine;

// Automatically attached to the persistent RunManager. Session progress only.
public class BuildingProgressManager : MonoBehaviour
{
    public static BuildingProgressManager Instance { get; private set; }
    public event Action ProgressChanged;

    [Header("Building catalog - assign all seven definitions")]
    [SerializeField] private List<BuildingDefinition> buildingDefinitions = new List<BuildingDefinition>();
    private readonly Queue<BuildingDefinition> pendingNotifications = new Queue<BuildingDefinition>();
    public BuildingDefinition[] GetBuildingDefinitions() => buildingDefinitions.ToArray();

    public bool HasPendingNotifications => pendingNotifications.Count > 0;

    public BuildingDefinition PeekNotification()
    {
        return pendingNotifications.Count > 0 ? pendingNotifications.Peek() : null;
    }

    public void ConfirmNotification(BuildingDefinition definition)
    {
        if (pendingNotifications.Count > 0 && pendingNotifications.Peek() == definition)
            pendingNotifications.Dequeue();
    }

    private void NotifyProgress()
    {
        // Check even while Surface objects are absent. The catalog belongs to RunManager.
        foreach (BuildingDefinition definition in buildingDefinitions.ToArray())
            IsUnlocked(definition);
        ProgressChanged?.Invoke();
    }

    [Header("Session progress")]
    [SerializeField] private int silverBlocks;
    [SerializeField] private int stoneBlocks;
    [SerializeField] private int completedRuns;
    [SerializeField] private bool smithHammerFound;
    [SerializeField] private List<string> completedQuestIds = new List<string>();
    [SerializeField] private List<ArtifactType> discoveredArtifacts = new List<ArtifactType>();
    [SerializeField] private List<string> builtIds = new List<string>();

    [Header("Smith hammer drop - initial balancing values")]
    [SerializeField, Range(0f, 1f)] private float smithHammerChance = 0.01f;
    [SerializeField, Min(1)] private int guaranteedHammerAfterBlocks = 200;
    [SerializeField] private int blocksWithoutHammer;

    public int SilverBlocks => silverBlocks;
    public int StoneBlocks => stoneBlocks;
    public int CompletedRuns => completedRuns;
    public bool SmithHammerFound => smithHammerFound;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Start()
    {
        // Supports an ArtifactManager created before this manager, including loaded finds.
        if (ArtifactManager.Instance != null)
            foreach (ArtifactType type in ArtifactManager.Instance.GetDiscoveredArtifacts())
                RegisterArtifact(type);
        NotifyProgress();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Call once AFTER removing a block. Returns true only on the hammer's first find.
    public bool RegisterDestroyedBlock(bool stone, bool silver)
    {
        if (RunManager.Instance == null || !RunManager.Instance.RunActive) return false;
        if (stone) stoneBlocks++;
        if (silver) silverBlocks++;
        bool foundNow = false;
        if (!smithHammerFound)
        {
            blocksWithoutHammer++;
            if (blocksWithoutHammer >= Mathf.Max(1, guaranteedHammerAfterBlocks) ||
                UnityEngine.Random.value < Mathf.Clamp01(smithHammerChance))
            {
                smithHammerFound = true;
                foundNow = true;
            }
        }
        NotifyProgress();
        return foundNow;
    }

    // RunManager calls this once after its runActive guard closes the run.
    public void RegisterCompletedRun()
    {
        completedRuns++;
        NotifyProgress();
    }

    public void RegisterArtifact(ArtifactType type)
    {
        if (type == ArtifactType.None || discoveredArtifacts.Contains(type)) return;
        discoveredArtifacts.Add(type);
        NotifyProgress();
    }

    // Future guild system: report only a successfully completed, accepted quest.
    // Use a unique completion ID per instance for repeatable quests.
    public void RegisterGuildQuestCompleted(string completionId)
    {
        if (string.IsNullOrWhiteSpace(completionId) || completedQuestIds.Contains(completionId)) return;
        completedQuestIds.Add(completionId);
        NotifyProgress();
    }

    public bool IsUnlocked(BuildingDefinition definition)
    {
        if (definition == null || string.IsNullOrWhiteSpace(definition.buildingId)) return false;
        if (!buildingDefinitions.Contains(definition)) buildingDefinitions.Add(definition);
        if (builtIds.Contains(definition.buildingId)) return true;
        var requirements = definition.requirements;
        if (requirements == null || requirements.Length == 0) return false;
        bool unlocked = !definition.anyRequirement;
        foreach (var requirement in requirements)
        {
            bool met = Meets(requirement);
            if (definition.anyRequirement) unlocked |= met;
            else unlocked &= met;
        }
        if (unlocked)
        {
            builtIds.Add(definition.buildingId);
            pendingNotifications.Enqueue(definition);
        }
        return unlocked;
    }

    private bool Meets(BuildingRequirement requirement)
    {
        if (requirement == null) return false;
        int amount = Mathf.Max(1, requirement.amount);
        switch (requirement.type)
        {
            case BuildingRequirementType.SilverBlocks: return silverBlocks >= amount;
            case BuildingRequirementType.StoneBlocks: return stoneBlocks >= amount;
            case BuildingRequirementType.CompletedRuns: return completedRuns >= amount;
            case BuildingRequirementType.SmithHammer: return smithHammerFound;
            case BuildingRequirementType.CompletedGuildQuests: return completedQuestIds.Count >= amount;
            case BuildingRequirementType.SpecificArtifact:
                return requirement.artifact != ArtifactType.None && discoveredArtifacts.Contains(requirement.artifact);
            case BuildingRequirementType.UniqueArtifacts: return discoveredArtifacts.Count >= amount;
            default: return false;
        }
    }

    [ContextMenu("TEST/Complete one guild quest")]
    private void TestGuildQuest()
    {
        if (Application.isPlaying) RegisterGuildQuestCompleted("debug-first-quest");
    }
}
