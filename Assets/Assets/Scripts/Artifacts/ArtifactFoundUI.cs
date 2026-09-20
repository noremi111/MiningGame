using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactFoundUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject artifactPanel;

    [Header("UI")]
    [SerializeField] private Image artifactImage;
    [SerializeField] private TMP_Text artifactNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text foundText;
    [SerializeField] private Button continueButton;

    [Header("Animation Target")]
    [SerializeField] private RectTransform artifactVisual;

    [Header("Animation")]
    [SerializeField] private float appearDuration = 0.35f;
    [SerializeField] private float startScale = 0.15f;
    [SerializeField] private float overshootScale = 1.20f;
    [SerializeField] private float finalScale = 1f;
    [SerializeField] private float startRotation = -15f;

    [Header("Artifact Sprites")]
    [SerializeField] private List<ArtifactSpriteEntry> artifactSprites =
        new List<ArtifactSpriteEntry>();

    [Header("UI Priority")]
    [SerializeField] private PerkSelectionUI perkSelectionUI;

    public bool IsOpen { get; private set; }

    [Header("Toolbox")]
    [SerializeField] private Sprite toolboxSprite;

    [Header("Magierturm")]
    [SerializeField] private Sprite mageTowerSprite;

    public void ShowMageTowerUnlocked()
    {
        Enqueue(new FoundItem
        {
            heading = "MAGIERTURM FREIGESCHALTET!",
            name = "Magierturm",
            description = "Du hast insgesamt 50 Kupfer abgebaut!\n\nDer Magierturm ist jetzt in der Oberwelt verfügbar.",
            sprite = mageTowerSprite
        });
    }

    private class FoundItem
    {
        public string heading;
        public string name;
        public string description;
        public Sprite sprite;
    }

    private readonly Queue<FoundItem> pendingItems = new Queue<FoundItem>();
    public bool HasPendingFinds => IsOpen || pendingItems.Count > 0 ||
        (artifactPanel != null && BuildingProgressManager.Instance != null &&
         BuildingProgressManager.Instance.HasPendingNotifications);
    private BuildingDefinition activeBuildingNotification;
    private BuildingProgressManager notificationOwner;
    private static ArtifactFoundUI visibleUI;

    private float previousTimeScale = 1f;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (artifactPanel != null)
            artifactPanel.SetActive(false);

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(
                CloseArtifactPanel
            );
        }
    }

    private void LateUpdate()
    {
        TryShowNext();
    }

    public void ShowArtifact(ArtifactData artifact)
    {
        if (artifact == null) return;
        Enqueue(new FoundItem
        {
            heading = "ARTEFAKT GEFUNDEN!",
            name = artifact.displayName,
            description = artifact.description,
            sprite = GetArtifactSprite(artifact.type)
        });
    }

    public void ShowToolbox()
    {
        Enqueue(new FoundItem
        {
            heading = "WERKZEUGKISTE GEFUNDEN!",
            name = "Werkzeugkiste",
            description = "Automatisch eingesammelt!\n\nBringe die Werkzeugkiste sicher zur Oberwelt zurück, um den Shop aufzubauen.",
            sprite = toolboxSprite
        });
    }

    private void Enqueue(FoundItem item)
    {
        if (artifactPanel == null)
        {
            Debug.LogWarning("ArtifactFoundUI: Fund-Panel fehlt.", this);
            return;
        }
        pendingItems.Enqueue(item);
    }

    private void TryShowNext()
    {
        if (IsOpen || artifactPanel == null || (visibleUI != null && visibleUI != this)) return;
        if (RunManager.Instance != null && RunManager.Instance.ReturningToSurface) return;
        if (perkSelectionUI != null && perkSelectionUI.IsOpen) return;
        // Do not open over an unrelated pause menu.
        if (Time.timeScale <= 0f) return;
        // Artifacts/toolbox already queued this frame take priority over building notices.
        if (pendingItems.Count > 0)
        {
            ShowNow(pendingItems.Dequeue());
            return;
        }
        var progress = BuildingProgressManager.Instance;
        if (progress == null || !progress.HasPendingNotifications) return;
        var definition = progress.PeekNotification();
        if (definition == null) return;
        activeBuildingNotification = definition;
        notificationOwner = progress;
        string title = string.IsNullOrWhiteSpace(definition.displayName)
            ? definition.name : definition.displayName;
        string description = string.IsNullOrWhiteSpace(definition.unlockDescription)
            ? title + " ist jetzt in der Oberwelt verfügbar." : definition.unlockDescription;
        if (!string.IsNullOrWhiteSpace(definition.effectDescription))
            description += "\n\n" + definition.effectDescription;
        ShowNow(new FoundItem
        {
            heading = "GEBÄUDE FREIGESCHALTET!",
            name = title,
            description = description,
            sprite = definition.buildingSprite
        });
    }

    private void ShowNow(FoundItem item)
    {
        previousTimeScale = Time.timeScale;
        IsOpen = true;
        visibleUI = this;
        artifactPanel.SetActive(true);
        Time.timeScale = 0f;

        if (foundText != null) foundText.text = item.heading;
        if (artifactNameText != null) artifactNameText.text = item.name;
        if (descriptionText != null) descriptionText.text = item.description;
        if (artifactImage != null)
        {
            artifactImage.sprite = item.sprite;
            artifactImage.enabled = item.sprite != null;
        }

        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(PlayArtifactAnimation());
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private IEnumerator PlayArtifactAnimation()
    {
        if (artifactVisual == null)
            yield break;

        artifactVisual.localScale =
            Vector3.one * startScale;

        artifactVisual.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                startRotation
            );

        // -----------------------------------------
        // Phase 1: großes Einpoppen
        // -----------------------------------------

        float timer = 0f;

        while (timer < appearDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    appearDuration
                );

            // SmoothStep
            t =
                t * t *
                (3f - 2f * t);

            float scale =
                Mathf.Lerp(
                    startScale,
                    overshootScale,
                    t
                );

            float rotation =
                Mathf.Lerp(
                    startRotation,
                    3f,
                    t
                );

            artifactVisual.localScale =
                Vector3.one * scale;

            artifactVisual.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation
                );

            yield return null;
        }

        // -----------------------------------------
        // Phase 2: leicht zurückfedern
        // -----------------------------------------

        timer = 0f;

        float settleDuration = 0.18f;

        while (timer < settleDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    settleDuration
                );

            float scale =
                Mathf.Lerp(
                    overshootScale,
                    finalScale,
                    t
                );

            float rotation =
                Mathf.Lerp(
                    3f,
                    0f,
                    t
                );

            artifactVisual.localScale =
                Vector3.one * scale;

            artifactVisual.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation
                );

            yield return null;
        }

        artifactVisual.localScale =
            Vector3.one *
            finalScale;

        artifactVisual.localRotation =
            Quaternion.identity;
    }

    // =========================================================
    // CLOSE
    // =========================================================

    public void CloseArtifactPanel()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        if (visibleUI == this) visibleUI = null;
        if (activeBuildingNotification != null && notificationOwner != null)
            notificationOwner.ConfirmNotification(activeBuildingNotification);
        activeBuildingNotification = null;
        notificationOwner = null;

        if (artifactPanel != null)
        {
            artifactPanel.SetActive(false);
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // Ein weiterhin offenes Perk-Menü darf nicht entpausiert werden.
        Time.timeScale = perkSelectionUI != null && perkSelectionUI.IsOpen
            ? 0f : previousTimeScale;
        TryShowNext();
    }

    // =========================================================
    // SPRITES
    // =========================================================

    private void OnDisable()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = null;
        bool wasOpen = IsOpen;
        IsOpen = false;
        if (visibleUI == this) visibleUI = null;
        // No confirmation here: an interrupted building notice stays in the persistent queue.
        activeBuildingNotification = null;
        notificationOwner = null;
        if (artifactPanel != null && artifactPanel != gameObject) artifactPanel.SetActive(false);
        if (wasOpen && Time.timeScale == 0f &&
            (perkSelectionUI == null || !perkSelectionUI.IsOpen)) Time.timeScale = previousTimeScale;
    }

    private void OnDestroy()
    {
        if (continueButton != null) continueButton.onClick.RemoveListener(CloseArtifactPanel);
    }

    private Sprite GetArtifactSprite(
        ArtifactType type)
    {
        foreach (
            ArtifactSpriteEntry entry
            in artifactSprites)
        {
            if (entry.type == type)
            {
                return entry.sprite;
            }
        }

        Debug.LogWarning(
            "Kein Sprite für Artefakt " +
            type +
            " gefunden."
        );

        return null;
    }
}