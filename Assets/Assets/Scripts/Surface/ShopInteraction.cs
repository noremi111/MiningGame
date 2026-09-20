using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopInteraction : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_Text interactionText;

    [Header("Shop")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Building Visuals (child objects only)")]
    [SerializeField] private GameObject ruinedShopVisual;
    [SerializeField] private GameObject rebuiltShopVisual;

    private bool playerInside;
    private bool shopOpen;
    private bool movementWasEnabled;

    private void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        RefreshBuildingVisuals();
        HideInteraction();
    }

    private void Update()
    {
        RefreshBuildingVisuals();
        if (!playerInside || shopOpen) return;
        ShowInteraction();
        if (Time.timeScale <= 0f || Keyboard.current == null) return;
        if (Keyboard.current.eKey.wasPressedThisFrame) Interact();
    }

    // Also usable by a future touch UI button.
    public void Interact()
    {
        if (!playerInside || shopOpen || Time.timeScale <= 0f) return;
        RunManager run = RunManager.Instance;
        if (run == null || run.RunActive) return;

        if (run.ShopUnlocked)
        {
            OpenShop();
            return;
        }

        if (run.TryRebuildShop())
        {
            RefreshBuildingVisuals();
            ShowInteraction();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        if (playerMovement == null)
            playerMovement = other.GetComponentInParent<PlayerMovement>();
        if (!shopOpen) ShowInteraction();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        if (shopOpen) CloseShop();
        HideInteraction();
    }

    private void OpenShop()
    {
        if (shopPanel == null)
        {
            Debug.LogWarning("ShopInteraction: Shop Panel fehlt.", this);
            return;
        }
        shopOpen = true;
        HideInteraction();
        shopPanel.SetActive(true);
        if (playerMovement != null)
        {
            movementWasEnabled = playerMovement.enabled;
            playerMovement.enabled = false;
        }
    }

    public void CloseShop()
    {
        bool wasOpen = shopOpen;
        shopOpen = false;
        if (shopPanel != null) shopPanel.SetActive(false);
        if (wasOpen && playerMovement != null)
            playerMovement.enabled = movementWasEnabled;
        if (playerInside) ShowInteraction();
        else HideInteraction();
    }

    private void OnDisable()
    {
        if (shopOpen) CloseShop();
        playerInside = false;
        HideInteraction();
    }

    private void RefreshBuildingVisuals()
    {
        bool unlocked = RunManager.Instance != null && RunManager.Instance.ShopUnlocked;
        SetVisual(ruinedShopVisual, !unlocked);
        SetVisual(rebuiltShopVisual, unlocked);
    }

    private void SetVisual(GameObject visual, bool visible)
    {
        // Never disable this interaction object or its ancestors.
        if (visual == null || transform.IsChildOf(visual.transform)) return;
        if (visual.activeSelf != visible) visual.SetActive(visible);
    }

    private void ShowInteraction()
    {
        if (interactionPanel != null) interactionPanel.SetActive(true);
        if (interactionText == null) return;
        RunManager run = RunManager.Instance;
        if (run == null)
            interactionText.text = "Shop nicht verfügbar: RunManager fehlt.";
        else if (run.ShopUnlocked)
            interactionText.text = "E - Shop öffnen";
        else if (run.HasSecuredToolbox)
            interactionText.text = "Werkzeugkiste gesichert!\nE - Shop wiederaufbauen";
        else
            interactionText.text = "Shop geschlossen\nFinde die verlorene Werkzeugkiste in der Mine\nund bringe sie sicher zurück.";
    }

    private void HideInteraction()
    {
        if (interactionPanel != null) interactionPanel.SetActive(false);
    }
}
