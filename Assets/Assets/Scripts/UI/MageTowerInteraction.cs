using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MageTowerInteraction : MonoBehaviour
{
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private GameObject artifactInventoryPanel;
    [SerializeField] private PlayerMovement playerMovement;

    private bool playerInside;
    private bool inventoryOpen;

    private void Start()
    {
        if (artifactInventoryPanel != null)
            artifactInventoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (!playerInside || inventoryOpen) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            OpenArtifactInventory();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        ShowInteraction();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        HideInteraction();
    }

    private void ShowInteraction()
    {
        if (inventoryOpen) return;
        if (interactionPanel != null) interactionPanel.SetActive(true);
        if (interactionText != null) interactionText.text = "E - Magierturm öffnen";
    }

    private void HideInteraction()
    {
        if (interactionPanel != null) interactionPanel.SetActive(false);
    }

    private void OpenArtifactInventory()
    {
        inventoryOpen = true;
        HideInteraction();
        if (artifactInventoryPanel != null) artifactInventoryPanel.SetActive(true);
        if (playerMovement != null) playerMovement.enabled = false;
    }

    public void CloseArtifactInventory()
    {
        inventoryOpen = false;
        if (artifactInventoryPanel != null) artifactInventoryPanel.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerInside) ShowInteraction();
    }
}
