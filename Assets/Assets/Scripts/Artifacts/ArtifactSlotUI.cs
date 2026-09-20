using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArtifactSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot")]
    [SerializeField] private int slotIndex;

    [Header("UI")]
    [SerializeField] private Image artifactImage;

    [Header("Inventory")]
    [SerializeField] private ArtifactInventoryUI inventoryUI;

    private ArtifactType currentType = ArtifactType.None;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || ArtifactManager.Instance == null) return;

        ArtifactDragItem draggedItem =
            eventData.pointerDrag.GetComponent<ArtifactDragItem>();

        if (draggedItem == null || draggedItem.ArtifactType == ArtifactType.None) return;

        if (ArtifactManager.Instance.EquipArtifactToSlot(draggedItem.ArtifactType, slotIndex))
        {
            inventoryUI?.ShowDescription(draggedItem.ArtifactType);
            inventoryUI?.RefreshUI();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentType != ArtifactType.None)
            inventoryUI?.ShowDescription(currentType);
    }

    public void Refresh(ArtifactType type, Sprite sprite)
    {
        currentType = type;

        if (artifactImage == null) return;

        if (type == ArtifactType.None || sprite == null)
        {
            artifactImage.sprite = null;
            artifactImage.enabled = false;
            return;
        }

        artifactImage.sprite = sprite;
        artifactImage.enabled = true;
        artifactImage.preserveAspect = true;
    }

    public void ClearSlot()
    {
        ArtifactManager.Instance?.UnequipSlot(slotIndex);
        inventoryUI?.RefreshUI();
    }
}
