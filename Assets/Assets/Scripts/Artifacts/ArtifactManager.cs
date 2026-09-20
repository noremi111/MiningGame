using System.Collections.Generic;
using UnityEngine;

public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager Instance { get; private set; }

    [Header("Artifact Slots")]
    [SerializeField] private int maxEquippedArtifacts = 3;

    private readonly HashSet<ArtifactType> discoveredArtifacts = new HashSet<ArtifactType>();
    private readonly List<ArtifactType> equippedArtifacts = new List<ArtifactType>();

    public int MaxEquippedArtifacts => maxEquippedArtifacts;
    public int DiscoveredCount => discoveredArtifacts.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureSlotCount();
    }

    private void EnsureSlotCount()
    {
        while (equippedArtifacts.Count < maxEquippedArtifacts)
            equippedArtifacts.Add(ArtifactType.None);

        while (equippedArtifacts.Count > maxEquippedArtifacts)
            equippedArtifacts.RemoveAt(equippedArtifacts.Count - 1);
    }

    public bool DiscoverArtifact(ArtifactType type)
    {
        if (type == ArtifactType.None || !discoveredArtifacts.Add(type))
            return false;

        BuildingProgressManager.Instance?.RegisterArtifact(type);
        ArtifactData artifact = ArtifactDatabase.Get(type);
        Debug.Log("ARTEFAKT ENTDECKT: " + (artifact != null ? artifact.displayName : type.ToString()));
        return true;
    }

    public bool IsDiscovered(ArtifactType type) =>
        type != ArtifactType.None && discoveredArtifacts.Contains(type);

    public bool IsEquipped(ArtifactType type) =>
        type != ArtifactType.None && equippedArtifacts.Contains(type);

    public IReadOnlyCollection<ArtifactType> GetDiscoveredArtifacts() => discoveredArtifacts;

    public IReadOnlyList<ArtifactType> GetEquippedArtifacts()
    {
        EnsureSlotCount();
        return equippedArtifacts;
    }

    public bool EquipArtifactToSlot(ArtifactType type, int slotIndex)
    {
        if (type == ArtifactType.None || !IsDiscovered(type))
            return false;

        EnsureSlotCount();

        if (slotIndex < 0 || slotIndex >= maxEquippedArtifacts)
            return false;

        for (int i = 0; i < equippedArtifacts.Count; i++)
            if (equippedArtifacts[i] == type)
                equippedArtifacts[i] = ArtifactType.None;

        equippedArtifacts[slotIndex] = type;
        return true;
    }

    public void UnequipSlot(int slotIndex)
    {
        EnsureSlotCount();
        if (slotIndex >= 0 && slotIndex < equippedArtifacts.Count)
            equippedArtifacts[slotIndex] = ArtifactType.None;
    }
}
