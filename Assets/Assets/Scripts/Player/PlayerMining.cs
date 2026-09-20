using System;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMining : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private LevelSystem levelSystem;
    [SerializeField] private MineGenerator mineGenerator;

    // =========================================================
    // VISUALS
    // =========================================================

    [Header("Visuals")]
    [SerializeField] private StoneEdgeController stoneEdgeController;

    // =========================================================
    // TILES
    // =========================================================

    [Header("Tiles")]
    [SerializeField] private TileBase bedrockTile;

    // =========================================================
    // MINING
    // =========================================================

    [Header("Magierturm - Kupfer")]
    [Tooltip("Alle Kupfer-Tile-Assets aus der Mine zuweisen. Ein zerstörter Kupferblock zählt als 1.")]
    [SerializeField] private TileBase[] copperTiles = new TileBase[0];

    [Header("Gebaeudefortschritt - Silbererz")]
    [Tooltip("Alle Silbererz-Tile-Assets zuweisen. Pro zerstoertem Block ein Fortschrittspunkt.")]
    [SerializeField] private TileBase[] silverTiles = new TileBase[0];

    [Header("Mining")]
    [SerializeField] private float miningRange = 1.5f;

    // =========================================================
    // GOLD POPUP
    // =========================================================

    [Header("Gold Popup")]
    [SerializeField] private GoldPopup goldPopupPrefab;
    [SerializeField] private Transform worldCanvas;

    // =========================================================
    // EFFECT POPUP
    // =========================================================

    [Header("Effect Popup")]
    [SerializeField] private EffectPopup effectPopupPrefab;

    // =========================================================
    // ARTIFACTS
    // =========================================================

    [Header("Artifact Drop")]
    [SerializeField] private float baseArtifactChance = 0.01f;
    [SerializeField] private float luckArtifactBonusPerPoint = 0.02f;

    [Header("Artifact UI")]
    [SerializeField] private ArtifactFoundUI artifactFoundUI;

    // =========================================================
    // PRIVATE
    // =========================================================

    private readonly Dictionary<Vector3Int, int>
        blockHealth =
            new Dictionary<Vector3Int, int>();

    [Header("Mining Animation")]
    [SerializeField] private Sprite pickaxeSprite;
    [SerializeField, Min(0.03f)] private float swingDuration = 0.12f;
    [SerializeField, Min(0f)] private float recoveryDuration = 0.06f;
    [SerializeField, Min(0.1f)] private float pickaxeSize = 0.8f;
    [SerializeField] private float pickaxeAngleOffset = -45f;
    [SerializeField, Min(0f)] private float shakeStrength = 0.035f;
    [SerializeField, Min(0.01f)] private float shakeDuration = 0.1f;
    [SerializeField, Range(0, 20)] private int fragmentCount = 8;
    [SerializeField, Min(0.05f)] private float fragmentLifetime = 0.4f;

    private bool isSwinging;
    private int miningGeneration;
    private readonly List<GameObject> miningVisuals = new List<GameObject>();
    private readonly Dictionary<Vector3Int, Color> hiddenTileColors = new Dictionary<Vector3Int, Color>();
    private readonly Dictionary<Vector3Int, TileFlags> hiddenTileFlags = new Dictionary<Vector3Int, TileFlags>();
    private Material fragmentMaterial;

    private Rigidbody2D playerRigidbody;
    private bool runFailureTriggered;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (playerStats == null)
        {
            playerStats =
                GetComponent<PlayerStats>();
        }

        if (levelSystem == null)
        {
            levelSystem =
                GetComponent<LevelSystem>();
        }

        playerRigidbody =
            GetComponent<Rigidbody2D>();

        // -----------------------------------------------------
        // EDGE CONTROLLER NOTFALLS AUTOMATISCH FINDEN
        // -----------------------------------------------------

        if (stoneEdgeController == null)
        {
            stoneEdgeController =
                FindFirstObjectByType<StoneEdgeController>();
        }

        if (stoneEdgeController == null)
        {
            Debug.LogWarning(
                "PlayerMining: StoneEdgeController wurde nicht gefunden!"
            );
        }
    }

    private void Update()
    {
        if (runFailureTriggered || isSwinging)
            return;

        if (Time.timeScale == 0f ||
            (artifactFoundUI != null && artifactFoundUI.HasPendingFinds))
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryMineTile();
        }
    }

    // =========================================================
    // TRY MINE
    // =========================================================

    private void TryMineTile()
    {
        if (groundTilemap == null ||
            mainCamera == null ||
            playerStats == null ||
            mineGenerator == null)
        {
            return;
        }

        Vector2 mouseScreen =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorld =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    mouseScreen.x,
                    mouseScreen.y,
                    0f
                )
            );

        mouseWorld.z = 0f;

        Vector3Int cell =
            groundTilemap.WorldToCell(
                mouseWorld
            );

        TileBase tile =
            groundTilemap.GetTile(
                cell
            );

        if (tile == null ||
            tile == bedrockTile)
        {
            return;
        }

        Vector3 tileCenter =
            groundTilemap.GetCellCenterWorld(
                cell
            );

        float distance =
            Vector2.Distance(
                transform.position,
                tileCenter
            );

        if (distance > miningRange)
            return;

        if (!playerStats.CanSpendEnergy(
            playerStats.EnergyCostPerHit))
        {
            TriggerEnergyFailure();
            return;
        }

        StartCoroutine(SwingAndMine(cell, tile));
    }

    private IEnumerator SwingAndMine(Vector3Int cell, TileBase tile)
    {
        isSwinging = true;
        int generation = miningGeneration;
        Vector3 target = groundTilemap.GetCellCenterWorld(cell);
        SpriteRenderer pick = null;
        if (pickaxeSprite != null)
        {
            pick = CreateMiningSprite("Mining Pickaxe", pickaxeSprite);
            float size = Mathf.Max(pickaxeSprite.bounds.size.x, pickaxeSprite.bounds.size.y);
            pick.transform.localScale = Vector3.one * pickaxeSize / Mathf.Max(size, 0.001f);
        }
        float duration = Mathf.Max(0.03f, swingDuration);
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            if (generation != miningGeneration) yield break;
            if (pick != null)
            {
                Vector3 direction = (target - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float t = Mathf.Clamp01(elapsed / duration);
                pick.transform.position = Vector3.Lerp(transform.position, target, 0.35f + 0.4f * t);
                pick.transform.rotation = Quaternion.Euler(0, 0, angle + pickaxeAngleOffset + Mathf.Lerp(85f, 0f, t * t));
            }
            yield return null;
        }
        if (pick != null) RemoveMiningVisual(pick.gameObject);
        if (generation != miningGeneration) yield break;

        // Revalidate after the wind-up. Cancelled swings cost no energy.
        if (groundTilemap != null && groundTilemap.GetTile(cell) == tile &&
            Vector2.Distance(transform.position, target) <= miningRange &&
            !runFailureTriggered &&
            (artifactFoundUI == null || !artifactFoundUI.HasPendingFinds))
        {
            if (playerStats.CanSpendEnergy(playerStats.EnergyCostPerHit))
                ApplyMiningHit(cell, tile, target);
            else
                TriggerEnergyFailure();
        }
        yield return new WaitForSeconds(Mathf.Max(0f, recoveryDuration));
        isSwinging = false;
    }

    private void ApplyMiningHit(Vector3Int cell, TileBase tile, Vector3 tileCenter)
    {
        MiningAudio.Current?.PlayHit(tile);

        playerStats.SpendEnergy(
            playerStats.EnergyCostPerHit
        );

        if (!blockHealth.ContainsKey(cell))
        {
            blockHealth[cell] =
                mineGenerator.GetBlockHealth(
                    tile
                );
        }

        int damage =
            CalculateMiningDamage(
                tileCenter
            );

        blockHealth[cell] -=
            damage;

        if (blockHealth[cell] <= 0)
        {
            DestroyBlock(
                cell,
                tile,
                tileCenter
            );
        }

        else
        {
            StartCoroutine(ShakeBlock(cell, tile));
        }

        // Fundanzeige auch beim letzten möglichen Schlag erst bestätigen lassen.
        StartCoroutine(CheckEnergyAfterFinds());
    }

    // =========================================================
    // DAMAGE
    // =========================================================

    private int CalculateMiningDamage(
        Vector3 hitPosition)
    {
        int damage =
            playerStats.MiningDamage;

        if (ArtifactManager.Instance == null)
            return damage;

        if (!ArtifactManager.Instance.IsEquipped(
            ArtifactType.CrimsonPickaxe))
        {
            return damage;
        }

        if (UnityEngine.Random.value <= 0.20f)
        {
            damage *= 2;
            MiningAudio.Current?.Play(MiningAudio.Cue.Critical);

            ShowEffectPopup(
                hitPosition,
                "CRITICAL HIT!\n×2 DAMAGE"
            );
        }

        return damage;
    }

    // =========================================================
    // DESTROY BLOCK
    // =========================================================

    private void DestroyBlock(
        Vector3Int cell,
        TileBase tile,
        Vector3 tileCenter)
    {
        int goldReward =
            mineGenerator.GetGoldReward(
                tile
            );

        // -----------------------------------------------------
        // STONE TAX
        // -----------------------------------------------------

        if (ArtifactManager.Instance != null &&
            ArtifactManager.Instance.IsEquipped(
                ArtifactType.StoneTax) &&
            mineGenerator.IsStoneTile(tile))
        {
            goldReward += 1;
        }

        if (goldReward > 0)
        {
            RunManager.Instance?.AddRunGold(
                goldReward
            );

            ShowGoldPopup(
                tileCenter,
                goldReward
            );
        }

        // -----------------------------------------------------
        // LUCKY COIN
        // -----------------------------------------------------

        TryLuckyCoin(
            tileCenter
        );

        // -----------------------------------------------------
        // XP
        // -----------------------------------------------------

        int xpReward =
            mineGenerator.GetXPReward(
                tile
            );

        if (xpReward > 0 &&
            levelSystem != null)
        {
            levelSystem.AddXP(
                xpReward
            );
        }

        // -----------------------------------------------------
        // ARTIFACT DROP
        // -----------------------------------------------------

        TryFindArtifact(
            tile
        );

        // =====================================================
        // BLOCK ENTFERNEN
        // =====================================================

        RestoreMiningTile(cell);
        SpawnMiningFragments(cell);
        MiningAudio.Current?.PlayBreak(tile);
        groundTilemap.SetTile(
            cell,
            null
        );

        blockHealth.Remove(
            cell
        );

        RegisterBuildingProgress(tile, cell);
        RegisterCopperForTower(tile, cell);
        TryFindShopToolbox(cell);

        if (playerRigidbody != null)
        {
            playerRigidbody.WakeUp();
        }

        // =====================================================
        // EDGE UPDATE
        // WICHTIG: NACH SetTile(null)!
        // =====================================================

        if (stoneEdgeController != null)
        {
            stoneEdgeController.RefreshAround(
                cell
            );
        }

        // =====================================================
        // STAIRS
        // =====================================================

        if (mineGenerator.HasHiddenStairs(
            cell))
        {
            MiningAudio.Current?.Play(MiningAudio.Cue.StairsRevealed);
            mineGenerator.RevealStairs(
                cell
            );
        }

        // =====================================================
        // SEISMIC PICK
        // =====================================================

        TrySeismicPick(
            cell
        );
    }

    // =========================================================
    // LUCKY COIN
    // =========================================================

    private void RegisterBuildingProgress(TileBase tile, Vector3Int cell)
    {
        if (tile == null || BuildingProgressManager.Instance == null) return;
        bool silver = silverTiles != null && Array.IndexOf(silverTiles, tile) >= 0;
        bool stone = !silver && mineGenerator.IsStoneTile(tile);
        if (!BuildingProgressManager.Instance.RegisterDestroyedBlock(stone, silver)) return;
        MiningAudio.Current?.Play(MiningAudio.Cue.SpecialFind);
        const string message = "SCHMIEDEHAMMER GEFUNDEN!\nSchmiede freigeschaltet!";
        ShowEffectPopup(groundTilemap.GetCellCenterWorld(cell), message);
        Debug.Log(message);
    }

    private void RegisterCopperForTower(TileBase tile, Vector3Int cell)
    {
        if (tile == null || copperTiles == null || RunManager.Instance == null) return;

        bool isCopper = false;
        foreach (TileBase copperTile in copperTiles)
        {
            if (copperTile == tile)
            {
                isCopper = true;
                break;
            }
        }

        if (!isCopper || !RunManager.Instance.RegisterCopperMinedForTower()) return;

        MiningAudio.Current?.Play(MiningAudio.Cue.SpecialFind);

        if (artifactFoundUI != null)
            artifactFoundUI.ShowMageTowerUnlocked();
        else
            ShowEffectPopup(groundTilemap.GetCellCenterWorld(cell),
                "MAGIERTURM FREIGESCHALTET!\n50 Kupfer abgebaut!");
    }

    private void TryFindShopToolbox(Vector3Int cell)
    {
        if (RunManager.Instance == null ||
            !RunManager.Instance.RegisterMinedBlockForShop()) return;

        MiningAudio.Current?.Play(MiningAudio.Cue.SpecialFind);
        const string message = "WERKZEUGKISTE GEFUNDEN!\nAutomatisch eingesammelt – sicher zurückbringen";
        if (artifactFoundUI != null)
            artifactFoundUI.ShowToolbox();
        else
            ShowEffectPopup(groundTilemap.GetCellCenterWorld(cell), message);
        Debug.Log(message);
    }

    private void TryLuckyCoin(
        Vector3 position)
    {
        if (ArtifactManager.Instance == null)
            return;

        if (!ArtifactManager.Instance.IsEquipped(
            ArtifactType.LuckyCoin))
        {
            return;
        }

        if (UnityEngine.Random.value > 0.05f)
            return;

        MiningAudio.Current?.Play(MiningAudio.Cue.LuckyCoin);
        const int bonusGold = 50;

        RunManager.Instance?.AddRunGold(
            bonusGold
        );

        ShowGoldPopup(
            position,
            bonusGold
        );

        ShowEffectPopup(
            position +
            Vector3.up * 0.35f,
            "LUCKY COIN!\n+50 GOLD"
        );
    }

    // =========================================================
    // SEISMIC PICK
    // =========================================================

    private void TrySeismicPick(
        Vector3Int centerCell)
    {
        if (ArtifactManager.Instance == null)
            return;

        if (!ArtifactManager.Instance.IsEquipped(
            ArtifactType.SeismicPick))
        {
            return;
        }

        Vector3Int[] directions =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (Vector3Int direction in directions)
        {
            Vector3Int neighbour =
                centerCell +
                direction;

            TileBase tile =
                groundTilemap.GetTile(
                    neighbour
                );

            if (!mineGenerator.IsStoneTile(tile))
                continue;

            DestroySeismicStone(
                neighbour,
                tile
            );
        }

        // Sicherheitshalber Bereich um ursprünglichen
        // Block nochmal aktualisieren.
        if (stoneEdgeController != null)
        {
            stoneEdgeController.RefreshAround(
                centerCell
            );
        }
    }

    private void DestroySeismicStone(
        Vector3Int cell,
        TileBase tile)
    {
        Vector3 center =
            groundTilemap.GetCellCenterWorld(
                cell
            );

        // -----------------------------------------------------
        // STONE TAX
        // -----------------------------------------------------

        if (ArtifactManager.Instance != null &&
            ArtifactManager.Instance.IsEquipped(
                ArtifactType.StoneTax))
        {
            RunManager.Instance?.AddRunGold(
                1
            );

            ShowGoldPopup(
                center,
                1
            );
        }

        // -----------------------------------------------------
        // LUCKY COIN
        // -----------------------------------------------------

        TryLuckyCoin(
            center
        );

        // -----------------------------------------------------
        // XP
        // -----------------------------------------------------

        int xp =
            mineGenerator.GetXPReward(
                tile
            );

        if (xp > 0 &&
            levelSystem != null)
        {
            levelSystem.AddXP(
                xp
            );
        }

        // =====================================================
        // BLOCK ENTFERNEN
        // =====================================================

        RestoreMiningTile(cell);
        SpawnMiningFragments(cell);
        MiningAudio.Current?.PlayBreak(tile);
        groundTilemap.SetTile(
            cell,
            null
        );

        blockHealth.Remove(
            cell
        );

        RegisterBuildingProgress(tile, cell);
        RegisterCopperForTower(tile, cell);
        TryFindShopToolbox(cell);

        // =====================================================
        // EDGE UPDATE
        // =====================================================

        if (stoneEdgeController != null)
        {
            stoneEdgeController.RefreshAround(
                cell
            );
        }

        // =====================================================
        // STAIRS
        // =====================================================

        if (mineGenerator.HasHiddenStairs(
            cell))
        {
            MiningAudio.Current?.Play(MiningAudio.Cue.StairsRevealed);
            mineGenerator.RevealStairs(
                cell
            );
        }
    }

    // =========================================================
    // ARTIFACT DROP
    // =========================================================

    private void TryFindArtifact(
        TileBase tile)
    {
        if (ArtifactManager.Instance == null ||
            playerStats == null ||
            mineGenerator == null)
        {
            return;
        }

        if (!mineGenerator.IsOreTile(tile))
            return;

        float chance =
            Mathf.Clamp01(
                baseArtifactChance *
                (
                    1f +
                    playerStats.Luck *
                    luckArtifactBonusPerPoint
                )
            );

        if (UnityEngine.Random.value >
            chance)
        {
            return;
        }

        TryDiscoverRandomArtifact();
    }

    private void TryDiscoverRandomArtifact()
    {
        if (ArtifactManager.Instance == null)
            return;

        Array values =
            Enum.GetValues(
                typeof(ArtifactType)
            );

        List<ArtifactType> available =
            new List<ArtifactType>();

        foreach (ArtifactType type in values)
        {
            if (type == ArtifactType.None)
                continue;

            if (!ArtifactManager.Instance
                .IsDiscovered(type))
            {
                available.Add(
                    type
                );
            }
        }

        if (available.Count == 0)
            return;

        ArtifactType found =
            available[
                UnityEngine.Random.Range(
                    0,
                    available.Count
                )
            ];

        if (!ArtifactManager.Instance
            .DiscoverArtifact(found))
        {
            return;
        }

        MiningAudio.Current?.Play(MiningAudio.Cue.Artifact);

        ArtifactData data =
            ArtifactDatabase.Get(
                found
            );

        if (data != null &&
            artifactFoundUI != null)
        {
            artifactFoundUI.ShowArtifact(
                data
            );
        }
    }

    // =========================================================
    // GOLD POPUP
    // =========================================================

    private void ShowGoldPopup(
        Vector3 position,
        int amount)
    {
        if (goldPopupPrefab == null)
            return;

        position +=
            Vector3.up * 0.25f;

        GoldPopup popup;

        if (worldCanvas != null)
        {
            popup =
                Instantiate(
                    goldPopupPrefab,
                    position,
                    Quaternion.identity,
                    worldCanvas
                );
        }
        else
        {
            popup =
                Instantiate(
                    goldPopupPrefab,
                    position,
                    Quaternion.identity
                );
        }

        popup.Setup(
            amount
        );
    }

    // =========================================================
    // EFFECT POPUP
    // =========================================================

    private void ShowEffectPopup(
        Vector3 position,
        string message)
    {
        if (effectPopupPrefab == null)
            return;

        position +=
            Vector3.up * 0.45f;

        EffectPopup popup;

        if (worldCanvas != null)
        {
            popup =
                Instantiate(
                    effectPopupPrefab,
                    position,
                    Quaternion.identity,
                    worldCanvas
                );
        }
        else
        {
            popup =
                Instantiate(
                    effectPopupPrefab,
                    position,
                    Quaternion.identity
                );
        }

        popup.Setup(
            message
        );
    }

    // =========================================================
    // ENERGY
    // =========================================================

    private IEnumerator CheckEnergyAfterFinds()
    {
        while (Time.timeScale == 0f ||
               (artifactFoundUI != null && artifactFoundUI.HasPendingFinds))
        {
            yield return null;
        }

        CheckEnergyFailure();
    }

    private void CheckEnergyFailure()
    {
        if (playerStats == null)
            return;

        if (!playerStats.CanSpendEnergy(
            playerStats.EnergyCostPerHit))
        {
            TriggerEnergyFailure();
        }
    }

    private void TriggerEnergyFailure()
    {
        if (runFailureTriggered)
            return;

        runFailureTriggered = true;

        enabled = false;

        PlayerMovement movement =
            GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.enabled = false;
        }

        if (RunManager.Instance != null)
        {
            RunManager.Instance.FailRun();
        }
    }

    // =========================================================
    // RESET BLOCK HP
    // =========================================================

    public void ClearBlockHealth()
    {
        ResetMiningVisuals();
        blockHealth.Clear();
    }

    private SpriteRenderer CreateMiningSprite(string objectName, Sprite sprite)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(transform, true);
        miningVisuals.Add(go);
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        TilemapRenderer tiles = groundTilemap.GetComponent<TilemapRenderer>();
        if (tiles != null)
        {
            renderer.sortingLayerID = tiles.sortingLayerID;
            renderer.sortingOrder = tiles.sortingOrder + 5;
        }
        return renderer;
    }

    private IEnumerator ShakeBlock(Vector3Int cell, TileBase tile)
    {
        Sprite sprite = groundTilemap.GetSprite(cell);
        if (sprite == null || hiddenTileColors.ContainsKey(cell)) yield break;
        Color color = groundTilemap.GetColor(cell);
        TileFlags flags = groundTilemap.GetTileFlags(cell);
        hiddenTileColors[cell] = color;
        hiddenTileFlags[cell] = flags;
        SpriteRenderer visual = CreateMiningSprite("Mining Hit", sprite);
        visual.color = color * groundTilemap.color;
        // Match tile anchor and transform without moving the tile collider.
        Vector3 origin = groundTilemap.CellToLocalInterpolated((Vector3)cell + groundTilemap.tileAnchor);
        Matrix4x4 matrix = groundTilemap.GetTransformMatrix(cell);
        visual.transform.SetParent(groundTilemap.transform, false);
        visual.transform.localPosition = origin + matrix.MultiplyPoint3x4(Vector3.zero);
        visual.transform.localRotation = matrix.rotation;
        visual.transform.localScale = matrix.lossyScale;
        Vector3 position = visual.transform.position;
        groundTilemap.SetTileFlags(cell, flags & ~TileFlags.LockColor);
        groundTilemap.SetColor(cell, new Color(color.r, color.g, color.b, 0f));
        float duration = Mathf.Max(0.01f, shakeDuration);
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            if (groundTilemap.GetTile(cell) != tile) break;
            float offset = Mathf.Sin(elapsed / duration * Mathf.PI * 6f) * shakeStrength * (1f - elapsed / duration);
            visual.transform.position = position + new Vector3(offset, offset * 0.35f, 0f);
            yield return null;
        }
        RestoreMiningTile(cell);
        RemoveMiningVisual(visual.gameObject);
    }

    private void RestoreMiningTile(Vector3Int cell)
    {
        if (!hiddenTileColors.TryGetValue(cell, out Color color)) return;
        if (groundTilemap != null)
        {
            groundTilemap.SetColor(cell, color);
            groundTilemap.SetTileFlags(cell, hiddenTileFlags[cell]);
        }
        hiddenTileColors.Remove(cell);
        hiddenTileFlags.Remove(cell);
    }

    private void SpawnMiningFragments(Vector3Int cell)
    {
        Sprite sprite = groundTilemap.GetSprite(cell);
        if (sprite == null || fragmentCount == 0) return;
        if (fragmentMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) return;
            fragmentMaterial = new Material(shader);
        }
        Vector2[] vertices = sprite.vertices;
        Vector2[] uv = sprite.uv;
        ushort[] triangles = sprite.triangles;
        if (triangles.Length < 3) return;
        Color color = groundTilemap.GetColor(cell) * groundTilemap.color;
        TilemapRenderer tiles = groundTilemap.GetComponent<TilemapRenderer>();
        for (int i = 0; i < fragmentCount; i++)
        {
            int index = UnityEngine.Random.Range(0, triangles.Length / 3) * 3;
            int a = triangles[index], b = triangles[index + 1], c = triangles[index + 2];
            Vector2 center = (vertices[a] + vertices[b] + vertices[c]) / 3f;
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[] { vertices[a] - center, vertices[b] - center, vertices[c] - center };
            mesh.uv = new Vector2[] { uv[a], uv[b], uv[c] };
            mesh.colors = new Color[] { color, color, color };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.RecalculateBounds();
            GameObject go = new GameObject("Mining Fragment");
            go.transform.SetParent(transform, true);
            go.transform.position = groundTilemap.GetCellCenterWorld(cell);
            go.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.1f, 0.23f);
            miningVisuals.Add(go);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = fragmentMaterial;
            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            properties.SetTexture("_MainTex", sprite.texture);
            renderer.SetPropertyBlock(properties);
            if (tiles != null)
            {
                renderer.sortingLayerID = tiles.sortingLayerID;
                renderer.sortingOrder = tiles.sortingOrder + 6;
            }
            StartCoroutine(AnimateMiningFragment(go, mesh));
        }
    }

    private IEnumerator AnimateMiningFragment(GameObject go, Mesh mesh)
    {
        Vector3 origin = go.transform.position;
        Vector2 velocity = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(1f, 2.5f);
        float spin = UnityEngine.Random.Range(-480f, 480f);
        float duration = Mathf.Max(0.05f, fragmentLifetime);
        Vector3 scale = go.transform.localScale;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            go.transform.position = origin + (Vector3)(velocity * elapsed) + Vector3.down * (2f * elapsed * elapsed);
            go.transform.Rotate(0f, 0f, spin * Time.deltaTime);
            go.transform.localScale = scale * (1f - elapsed / duration);
            yield return null;
        }
        RemoveMiningVisual(go);
    }

    private void RemoveMiningVisual(GameObject go)
    {
        if (go == null) return;
        MeshFilter filter = go.GetComponent<MeshFilter>();
        if (filter != null && filter.sharedMesh != null) Destroy(filter.sharedMesh);
        miningVisuals.Remove(go);
        Destroy(go);
    }

    private void ResetMiningVisuals()
    {
        miningGeneration++;
        StopAllCoroutines();
        isSwinging = false;
        foreach (Vector3Int cell in new List<Vector3Int>(hiddenTileColors.Keys)) RestoreMiningTile(cell);
        foreach (GameObject go in new List<GameObject>(miningVisuals)) RemoveMiningVisual(go);
        miningVisuals.Clear();
    }

    private void OnDisable()
    {
        ResetMiningVisuals();
    }

    private void OnDestroy()
    {
        if (fragmentMaterial != null) Destroy(fragmentMaterial);
    }
}
