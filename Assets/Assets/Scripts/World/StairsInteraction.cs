using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class StairsInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Tilemap stairsTilemap;
    [SerializeField] private TileBase stairsDownTile;
    [SerializeField] private TileBase stairsUpTile;
    [SerializeField] private MineGenerator mineGenerator;
    [SerializeField] private PerkSelectionUI perkSelectionUI;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 0.9f;

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_Text interactionText;

    [Header("Exit Confirmation")]
    [SerializeField] private GameObject exitMinePanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    
    [Header("artifact")]
    [SerializeField] private ArtifactFoundUI artifactFoundUI;

    private Vector3Int currentStairCell;

    private bool canInteract;
    private bool currentStairsGoUp;
    private bool confirmationOpen;

    private void Start()
    {
        ConfigureInteractionHint();

        if (interactionPanel != null)
            interactionPanel.SetActive(false);

        if (exitMinePanel != null)
            exitMinePanel.SetActive(false);

        if (yesButton != null)
            yesButton.onClick.AddListener(ConfirmExitMine);

        if (noButton != null)
            noButton.onClick.AddListener(CancelExitMine);
    }

    private void Update()
    {
        if (perkSelectionUI != null &&
            perkSelectionUI.IsOpen)
        {
            canInteract = false;
            HideInteraction();
            return;
        }

        if (artifactFoundUI != null &&
            artifactFoundUI.IsOpen)
        {
            canInteract = false;
            HideInteraction();
            return;
        }
        
        // Perk-Auswahl hat höchste Priorität.
        if (perkSelectionUI != null &&
            perkSelectionUI.IsOpen)
        {
            canInteract = false;
            HideInteraction();
            return;
        }

        if (confirmationOpen)
            return;

        CheckForStairs();

        if (!canInteract)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            UseStairs();
        }
    }

    private void CheckForStairs()
    {
        canInteract = false;

        if (player == null ||
            stairsTilemap == null)
        {
            HideInteraction();
            return;
        }

        Vector3Int playerCell =
            stairsTilemap.WorldToCell(
                player.position
            );

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int cell =
                    playerCell +
                    new Vector3Int(x, y, 0);

                TileBase tile =
                    stairsTilemap.GetTile(cell);

                bool isDown =
                    tile == stairsDownTile;

                bool isUp =
                    tile == stairsUpTile;

                if (!isDown && !isUp)
                    continue;

                Vector3 center =
                    stairsTilemap.GetCellCenterWorld(
                        cell
                    );

                float distance =
                    Vector2.Distance(
                        player.position,
                        center
                    );

                if (distance > interactionRange)
                    continue;

                currentStairCell = cell;
                currentStairsGoUp = isUp;
                canInteract = true;

                ShowInteraction();

                return;
            }
        }

        HideInteraction();
    }

    private void UseStairs()
    {
        if (!canInteract)
            return;

        if (currentStairsGoUp)
        {
            OpenExitConfirmation();
        }
        else
        {
            GoDown();
        }
    }

    // =========================================================
    // DOWN
    // =========================================================

    private void GoDown()
    {
        HideInteraction();

        if (mineGenerator != null)
        {
            canInteract = false;
            mineGenerator.GoToNextFloor();
            MiningAudio.Current?.Play(MiningAudio.Cue.StairsUse);
        }
    }

    // =========================================================
    // EXIT CONFIRMATION
    // =========================================================

    private void OpenExitConfirmation()
    {
        confirmationOpen = true;

        HideInteraction();

        if (exitMinePanel != null)
            exitMinePanel.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    private void ConfirmExitMine()
    {
        if (!confirmationOpen) return;
        confirmationOpen = false;

        if (exitMinePanel != null)
            exitMinePanel.SetActive(false);

        if (RunManager.Instance != null && RunManager.Instance.RunActive)
        {
            // RunManager stops existing sounds before loading Surface.
            MiningAudio.Current?.PrepareStairsReturn();
            RunManager.Instance.ReturnSafely();
        }
    }

    private void CancelExitMine()
    {
        confirmationOpen = false;

        if (exitMinePanel != null)
            exitMinePanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        CheckForStairs();
    }

    // =========================================================
    // UI
    // =========================================================

    // Der reine E-Hinweis darf PlayerMinings UI-Abfrage nicht blockieren.
    private void ConfigureInteractionHint()
    {
        if (interactionPanel != null)
        {
            foreach (Graphic graphic in
                     interactionPanel.GetComponentsInChildren<Graphic>(true))
            {
                MakeHintClickThrough(graphic);
            }
        }

        // Auch dann berücksichtigen, wenn der Text außerhalb des Panels liegt.
        if (interactionText != null)
            MakeHintClickThrough(interactionText);
    }

    private void MakeHintClickThrough(Graphic graphic)
    {
        // Die separate Austrittsbestätigung muss weiterhin Klicks empfangen.
        if (exitMinePanel != null &&
            graphic.transform.IsChildOf(exitMinePanel.transform))
            return;

        graphic.raycastTarget = false;
    }

    private void ShowInteraction()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(true);

        if (interactionText == null)
            return;

        if (currentStairsGoUp)
        {
            interactionText.text =
                "E - Mine verlassen";
        }
        else
        {
            interactionText.text =
                "E - Nächste Etage";
        }
    }

    private void HideInteraction()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }
}