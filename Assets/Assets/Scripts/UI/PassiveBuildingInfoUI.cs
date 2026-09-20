using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassiveBuildingInfoUI : MonoBehaviour
{
    public BuildingDefinition definition;
    public BuildingInteraction interaction;
    public TMP_Text titleText;
    public TMP_Text effectText;
    [Tooltip("Aus: vorhandenen EffectText behalten. An: nichtleeren Text aus der Definition uebernehmen.")]
    public bool useDefinitionEffectText = false;
    public Image buildingImage;
    public Button closeButton;
    private void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }
    private void OnEnable() { Refresh(); }
    public void Refresh()
    {
        if (definition == null) return;
        if (titleText != null) titleText.text = string.IsNullOrWhiteSpace(definition.displayName)
            ? definition.name : definition.displayName;
        // Never replace authored text with an empty definition description.
        // By default a text entered directly in the panel takes priority.
        if (effectText != null && !string.IsNullOrWhiteSpace(definition.effectDescription) &&
            (useDefinitionEffectText || string.IsNullOrWhiteSpace(effectText.text)))
        {
            effectText.text = definition.effectDescription;
        }
        if (buildingImage != null)
        {
            buildingImage.sprite = definition.buildingSprite;
            buildingImage.enabled = definition.buildingSprite != null;
            buildingImage.preserveAspect = true;
        }
    }
    public void Close()
    {
        if (interaction != null) interaction.ClosePanel();
        else gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
    }
}
