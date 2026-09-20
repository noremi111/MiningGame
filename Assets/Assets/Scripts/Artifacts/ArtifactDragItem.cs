using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArtifactDragItem :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image artifactImage;
    [SerializeField] private TMP_Text nameText;

    private ArtifactType artifactType;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalAnchoredPosition;
    private ArtifactInventoryUI inventoryUI;

    public ArtifactType ArtifactType => artifactType;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        inventoryUI = GetComponentInParent<ArtifactInventoryUI>();
    }

    public void Setup(ArtifactType type, Sprite sprite, string displayName, string description)
    {
        artifactType = type;

        if (artifactImage != null)
        {
            artifactImage.sprite = sprite;
            artifactImage.enabled = sprite != null;
            artifactImage.preserveAspect = true;
        }

        if (nameText != null)
            nameText.text = displayName;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (artifactType != ArtifactType.None && inventoryUI != null)
            inventoryUI.ShowDescription(artifactType);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (artifactType == ArtifactType.None || rectTransform == null) return;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        inventoryUI?.ShowDescription(artifactType);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (artifactType == ArtifactType.None || rectTransform == null || canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        if (rectTransform != null) rectTransform.anchoredPosition = originalAnchoredPosition;
    }
}
