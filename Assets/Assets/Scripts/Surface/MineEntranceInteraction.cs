using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MineEntranceInteraction : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_Text interactionText;

    private bool playerInside;

    private void Start()
    {
        HideInteraction();
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartRun();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        ShowInteraction();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        HideInteraction();
    }

    private void StartRun()
    {
        HideInteraction();

        if (RunManager.Instance != null)
        {
            RunManager.Instance.StartNewRun();
        }
        else
        {
            Debug.LogError("RunManager wurde nicht gefunden!");
        }
    }

    private void ShowInteraction()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(true);

        if (interactionText != null)
            interactionText.text = "E - Mine betreten";
    }

    private void HideInteraction()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }
}