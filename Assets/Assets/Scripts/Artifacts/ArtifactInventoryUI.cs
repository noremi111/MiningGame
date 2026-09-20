using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ArtifactInventoryUI : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private Transform inventoryContent;
    [SerializeField] private GameObject artifactItemPrefab;

    [Header("Equipped Slots")]
    [SerializeField] private ArtifactSlotUI[] equippedSlots;

    [Header("Description")]
    [SerializeField] private TMP_Text artifactNameText;
    [SerializeField] private TMP_Text artifactDescriptionText;

    [Header("Artifact Sprites")]
    [SerializeField] private List<ArtifactSpriteEntry> artifactSprites = new List<ArtifactSpriteEntry>();

    private void OnEnable() => StartCoroutine(RefreshNextFrame());

    private IEnumerator RefreshNextFrame()
    {
        yield return null;
        if (ArtifactManager.Instance != null)
            RefreshUI();
    }

    public void RefreshUI()
    {
        RefreshInventory();
        RefreshEquippedSlots();
    }

    private void RefreshInventory()
    {
        if (ArtifactManager.Instance == null || inventoryContent == null || artifactItemPrefab == null)
            return;

        for (int i = inventoryContent.childCount - 1; i >= 0; i--)
            Destroy(inventoryContent.GetChild(i).gameObject);

        IReadOnlyCollection<ArtifactType> discovered =
            ArtifactManager.Instance.GetDiscoveredArtifacts();

        foreach (ArtifactType type in discovered)
        {
            if (type == ArtifactType.None) continue;

            ArtifactData data = ArtifactDatabase.Get(type);
            if (data == null) continue;

            GameObject itemObject = Instantiate(artifactItemPrefab, inventoryContent);
            ArtifactDragItem dragItem = itemObject.GetComponent<ArtifactDragItem>();

            if (dragItem == null)
            {
                Destroy(itemObject);
                continue;
            }

            dragItem.Setup(type, GetArtifactSprite(type), data.displayName, data.description);
        }
    }

    private void RefreshEquippedSlots()
    {
        if (ArtifactManager.Instance == null || equippedSlots == null)
            return;

        IReadOnlyList<ArtifactType> equipped =
            ArtifactManager.Instance.GetEquippedArtifacts();

        for (int i = 0; i < equippedSlots.Length; i++)
        {
            if (equippedSlots[i] == null) continue;

            ArtifactType type = i < equipped.Count ? equipped[i] : ArtifactType.None;
            Sprite sprite = type != ArtifactType.None ? GetArtifactSprite(type) : null;
            equippedSlots[i].Refresh(type, sprite);
        }
    }

    public void ShowDescription(ArtifactType type)
    {
        if (type == ArtifactType.None) return;

        ArtifactData data = ArtifactDatabase.Get(type);
        if (data == null) return;

        if (artifactNameText != null) artifactNameText.text = data.displayName;
        if (artifactDescriptionText != null) artifactDescriptionText.text = data.description;
    }

    public Sprite GetArtifactSprite(ArtifactType type)
    {
        foreach (ArtifactSpriteEntry entry in artifactSprites)
            if (entry != null && entry.type == type)
                return entry.sprite;

        return null;
    }
}
