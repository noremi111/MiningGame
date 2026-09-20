using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CartographersEyeEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MineGenerator mineGenerator;
    [SerializeField] private Tilemap stairsPreviewTilemap;
    [SerializeField] private TileBase stairsPreviewTile;

    [Header("UI")]
    [SerializeField] private ArtifactEffectBanner effectBanner;

    [Header("Timing")]
    [SerializeField] private float revealInterval = 60f;
    [SerializeField] private float revealDuration = 5f;

    private float timer;
    private bool revealRunning;

    private Coroutine revealCoroutine;

    private readonly List<Vector3Int> shownPositions =
        new List<Vector3Int>();

    private void Start()
    {
        ClearPreviewImmediate();
    }

    private void Update()
    {
        if (!IsArtifactActive())
        {
            timer = 0f;

            if (revealRunning)
            {
                ClearPreviewImmediate();
            }

            return;
        }

        if (revealRunning)
            return;

        timer += Time.deltaTime;

        if (timer >= revealInterval)
        {
            timer = 0f;

            revealCoroutine =
                StartCoroutine(
                    RevealHiddenStairs()
                );
        }
    }

    private bool IsArtifactActive()
    {
        if (ArtifactManager.Instance == null)
            return false;

        return ArtifactManager.Instance.IsEquipped(
            ArtifactType.CartographersEye
        );
    }

    private IEnumerator RevealHiddenStairs()
    {
        if (mineGenerator == null)
        {
            Debug.LogError(
                "CartographersEyeEffect: MineGenerator fehlt!"
            );

            yield break;
        }

        if (stairsPreviewTilemap == null)
        {
            Debug.LogError(
                "CartographersEyeEffect: StairsPreview Tilemap fehlt!"
            );

            yield break;
        }

        if (stairsPreviewTile == null)
        {
            Debug.LogError(
                "CartographersEyeEffect: StairsPreview Tile fehlt!"
            );

            yield break;
        }

        revealRunning = true;

        shownPositions.Clear();

        // Sicherheit:
        // Alte Preview-Tiles vorher entfernen.
        stairsPreviewTilemap.ClearAllTiles();

        int stairCount = 0;

        foreach (
            Vector3Int position
            in mineGenerator.GetHiddenStairPositions())
        {
            stairsPreviewTilemap.SetTile(
                position,
                stairsPreviewTile
            );

            shownPositions.Add(
                position
            );

            stairCount++;
        }

        if (effectBanner != null)
        {
            effectBanner.Show(
                "CARTOGRAPHER'S EYE",
                stairCount +
                " hidden stairs revealed!"
            );
        }

        Debug.Log(
            "CARTOGRAPHER'S EYE: " +
            stairCount +
            " versteckte Treppen angezeigt."
        );

        yield return new WaitForSeconds(
            revealDuration
        );

        ClearPreviewImmediate();

        revealCoroutine = null;
    }

    // =========================================================
    // SOFORT ALLE PREVIEWS ENTFERNEN
    // =========================================================

    public void ClearPreviewImmediate()
    {
        if (revealCoroutine != null)
        {
            StopCoroutine(
                revealCoroutine
            );

            revealCoroutine = null;
        }

        if (stairsPreviewTilemap != null)
        {
            stairsPreviewTilemap.ClearAllTiles();
        }

        shownPositions.Clear();

        revealRunning = false;

        Debug.Log(
            "Cartographer's Eye Preview entfernt."
        );
    }

    // =========================================================
    // BEI NEUER ETAGE
    // =========================================================

    public void OnFloorChanged()
    {
        ClearPreviewImmediate();

        // Neuer Floor beginnt wieder mit vollem Intervall.
        timer = 0f;
    }

    [ContextMenu("TEST - Reveal Stairs")]
    private void DebugReveal()
    {
        if (!Application.isPlaying)
            return;

        if (revealRunning)
            return;

        revealCoroutine =
            StartCoroutine(
                RevealHiddenStairs()
            );
    }
}