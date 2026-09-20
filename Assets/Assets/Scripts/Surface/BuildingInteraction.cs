using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Keep this component and its trigger on the always-active building root.
[DisallowMultipleComponent]
[RequireComponent(typeof(BuildingController))]
public class BuildingInteraction : MonoBehaviour
{
    public enum UnlockSource { Manual, ShopToolbox, MageTowerCopper, Conditions }

    [Header("Building")]
    [SerializeField] private string buildingName = "Building";
    [SerializeField] private UnlockSource unlockSource = UnlockSource.Manual;
    [SerializeField, Tooltip("Start value and live test switch for Manual only.")]
    private bool manualBuilt;
    [SerializeField] private BuildingDefinition definition;

    [Header("Panel (optional for passive buildings)")]
    [SerializeField] private GameObject buildingPanel;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private UnityEvent onPanelOpened;
    [SerializeField] private UnityEvent onPanelClosed;

    [Header("Interaction hint")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private string openText = "E - {0} öffnen";
    [SerializeField, TextArea] private string lockedText = "Gebäude noch nicht freigeschaltet.";
    [SerializeField, TextArea] private string rebuildText = "Werkzeugkiste gesichert!\nE - Shop wiederaufbauen";

    private static BuildingInteraction openOwner;
    private static BuildingInteraction hintOwner;
    private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();
    private BuildingController building;
    private RunManager observedRun;
    private BuildingProgressManager observedProgress;
    private UnlockSource observedSource;
    private bool bound;
    private bool panelOpen;
    private bool movementWasEnabled;
    public bool IsBuilt => building != null && building.IsBuilt;
    public bool IsPanelOpen => panelOpen;

    private void Start()
    {
        building = GetComponent<BuildingController>();
        // A panel must not contain this component: closing it would disable the building.
        if (buildingPanel != null && transform.IsChildOf(buildingPanel.transform))
        {
            Debug.LogError("BuildingInteraction: Panel must be outside the building root.", this);
            buildingPanel = null;
        }
        if (buildingPanel != null) buildingPanel.SetActive(false);
        if (interactionPanel != null && hintOwner == null) interactionPanel.SetActive(false);
        BindSource();
    }

    private void Update()
    {
        BindSource();
        if (unlockSource == UnlockSource.Manual && IsBuilt != manualBuilt) RefreshState();
        // Also restore movement if another menu script disabled the panel directly.
        if (panelOpen && (buildingPanel == null || !buildingPanel.activeInHierarchy)) ClosePanel();
        playerColliders.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
        if (playerColliders.Count == 0 || (observedRun != null && observedRun.RunActive))
        {
            if (panelOpen) ClosePanel();
            HideHint();
            return;
        }
        if (panelOpen || openOwner != null) return;
        ShowHint();
        if (hintOwner == this && Time.timeScale > 0f && Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame) Interact();
    }

    private void BindSource()
    {
        if (building == null) building = GetComponent<BuildingController>();
        RunManager current = RunManager.Instance;
        if (bound && ReferenceEquals(current, observedRun) &&
            ReferenceEquals(BuildingProgressManager.Instance, observedProgress) && observedSource == unlockSource) return;
        UnbindSource();
        observedRun = current;
        observedProgress = BuildingProgressManager.Instance;
        if (observedProgress != null) observedProgress.ProgressChanged += RefreshState;
        observedSource = unlockSource;
        bound = true;
        if (observedRun != null)
        {
            observedRun.ShopStateChanged += RefreshState;
            observedRun.MageTowerStateChanged += RefreshState;
        }
        RefreshState();
    }

    private void UnbindSource()
    {
        if (observedProgress != null) observedProgress.ProgressChanged -= RefreshState;
        observedProgress = null;
        if (observedRun != null)
        {
            observedRun.ShopStateChanged -= RefreshState;
            observedRun.MageTowerStateChanged -= RefreshState;
        }
        observedRun = null;
        bound = false;
    }

    private void RefreshState()
    {
        bool built = unlockSource == UnlockSource.Conditions
            ? observedProgress != null && observedProgress.IsUnlocked(definition)
            : unlockSource == UnlockSource.Manual ? manualBuilt :
            observedRun != null && (unlockSource == UnlockSource.ShopToolbox
                ? observedRun.ShopUnlocked : observedRun.MageTowerUnlocked);
        if (building != null) building.SetBuilt(built);
        if (!built && panelOpen) ClosePanel();
        if (hintOwner == this) ShowHint();
    }

    // For future unlock controllers / UnityEvents. Manual state is not saved here.
    public void SetBuilt(bool built)
    {
        if (unlockSource != UnlockSource.Manual)
        {
            Debug.LogWarning("BuildingInteraction: SetBuilt requires Manual unlock source.", this);
            return;
        }
        manualBuilt = built;
        if (building == null) building = GetComponent<BuildingController>();
        RefreshState();
    }

    public void Interact()
    {
        if (!isActiveAndEnabled || playerColliders.Count == 0 || panelOpen ||
            openOwner != null || Time.timeScale <= 0f) return;
        BindSource();
        if (observedRun != null && observedRun.RunActive) return;
        if (!IsBuilt)
        {
            if (unlockSource == UnlockSource.ShopToolbox && observedRun != null)
                observedRun.TryRebuildShop();
            return; // Building and opening remain two separate interactions.
        }
        if (buildingPanel == null) return;
        panelOpen = true;
        openOwner = this;
        HideHint();
        if (playerMovement != null)
        {
            movementWasEnabled = playerMovement.enabled;
            playerMovement.enabled = false;
        }
        buildingPanel.SetActive(true);
        onPanelOpened?.Invoke();
    }

    public void ClosePanel()
    {
        if (!panelOpen) return;
        panelOpen = false;
        if (openOwner == this) openOwner = null;
        if (buildingPanel != null) buildingPanel.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = movementWasEnabled;
        onPanelClosed?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();
        if (movement == null) return;
        if (playerMovement != null && playerMovement != movement) return;
        playerMovement = movement;
        playerColliders.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!playerColliders.Remove(other) || playerColliders.Count > 0) return;
        ClosePanel();
        HideHint();
    }

    private void ShowHint()
    {
        if (playerColliders.Count == 0 || panelOpen || openOwner != null) return;
        if (IsBuilt && buildingPanel == null) { HideHint(); return; }
        if (hintOwner != null && hintOwner != this) return;
        hintOwner = this;
        if (interactionPanel != null) interactionPanel.SetActive(true);
        if (interactionText != null)
            interactionText.text = IsBuilt ? openText.Replace("{0}", buildingName) :
                unlockSource == UnlockSource.ShopToolbox && observedRun != null && observedRun.HasSecuredToolbox
                    ? rebuildText : lockedText;
    }

    private void HideHint()
    {
        if (hintOwner != this) return;
        hintOwner = null;
        if (interactionPanel != null) interactionPanel.SetActive(false);
    }

    private void OnDisable()
    {
        UnbindSource();
        playerColliders.Clear();
        ClosePanel();
        HideHint();
    }
}
