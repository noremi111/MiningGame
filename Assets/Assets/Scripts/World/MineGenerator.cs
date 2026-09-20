using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MineGenerator : MonoBehaviour
{
    // =========================================================
    // TILEMAPS
    // =========================================================

    [Header("Tilemaps")]
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap stairsTilemap;

    [Header("Grass Decoration")]
    [SerializeField] private Tilemap backgroundDecorationTilemap;
    [SerializeField] private TileBase[] grassTiles;
    [SerializeField] private bool generateGrass = true;
    [Range(0f, 1f)]
    [SerializeField] private float grassClusterChance = 0.08f;
    [Min(1)] [SerializeField] private int grassClusterMinSize = 2;
    [Min(1)] [SerializeField] private int grassClusterMaxSize = 6;
    [Range(0, 4)] [SerializeField] private int grassDistanceFromRock = 1;
    [Range(0, 3)] [SerializeField] private int grassClusterSpacing = 1;



    // =========================================================
    // BACKGROUND / CAVE FLOOR
    // =========================================================

    [Header("Background Tiles")]
    [SerializeField] private TileBase[] caveBackgroundTiles;

    // =========================================================
    // STONE VARIATIONS
    // =========================================================

    [Header("Stone Variations")]
    [SerializeField] private TileBase[] stoneTiles;

    // =========================================================
    // BASIC TILES
    // =========================================================

    [Header("Basic Tiles")]
    [SerializeField] private TileBase bedrockTile;
    [SerializeField] private TileBase stairsDownTile;
    [SerializeField] private TileBase stairsUpTile;

    // =========================================================
    // ORE TILES
    // =========================================================

    [Header("Ore Tiles")]
    [SerializeField] private TileBase copperOreTile;
    [SerializeField] private TileBase silverOreTile;
    [SerializeField] private TileBase goldOreTile;
    [SerializeField] private TileBase rubyOreTile;
    [SerializeField] private TileBase diamondOreTile;

    // =========================================================
    // PLAYER
    // =========================================================

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMining playerMining;

    // =========================================================
    // VISUALS
    // =========================================================

    [Header("Fireflies")]
    [Tooltip("Optional component on this GameObject; found automatically if left empty.")]
    [SerializeField] private MineFireflies mineFireflies;

    [Header("Entrance Torch")]
    [SerializeField] private bool spawnEntranceTorch = true;
    [Tooltip("Torch prefab from the Project window, including Visual and TorchLight.")]
    [SerializeField] private GameObject entranceTorchPrefab;
    [Tooltip("Preferred cell relative to the entrance stairs. Falls back to another free neighbour.")]
    [SerializeField] private Vector2Int entranceTorchCellOffset = Vector2Int.right;
    [Tooltip("Fine adjustment in world units, e.g. to align the sprite base.")]
    [SerializeField] private Vector3 entranceTorchWorldOffset = Vector3.zero;

    [Header("Visuals")]
    [SerializeField] private StoneEdgeController stoneEdgeController;

    // =========================================================
    // ARTIFACT EFFECTS
    // =========================================================

    [Header("Artifact Effects")]
    [SerializeField] private CartographersEyeEffect cartographersEyeEffect;

    // =========================================================
    // MINE SIZE
    // =========================================================

    [Header("Mine Size")]
    [SerializeField] private int width = 60;
    [SerializeField] private int height = 60;
    [SerializeField] private int borderThickness = 2;
    [SerializeField] private int startAreaRadius = 3;

    // =========================================================
    // CAVE GENERATION
    // =========================================================

    [Header("Cave Generation")]
    [SerializeField] private float caveScale = 0.12f;

    [Range(0f, 1f)]
    [SerializeField] private float caveThreshold = 0.35f;

    // =========================================================
    // STAIRS
    // =========================================================

    [Header("Stairs")]
    [SerializeField] private int stairsPerFloor = 3;

    // =========================================================
    // FLOOR
    // =========================================================

    [Header("Floor")]
    [SerializeField] private int currentFloor = 1;

    public int CurrentFloor => currentFloor;

    // =========================================================
    // ORE UNLOCK FLOORS
    // =========================================================

    [Header("Ore Unlock Floors")]
    [SerializeField] private int copperUnlockFloor = 1;
    [SerializeField] private int silverUnlockFloor = 4;
    [SerializeField] private int goldUnlockFloor = 6;
    [SerializeField] private int rubyUnlockFloor = 10;
    [SerializeField] private int diamondUnlockFloor = 15;

    // =========================================================
    // VEIN SIZE
    // =========================================================

    [Header("Copper Veins")]
    [SerializeField] private int copperVeinMin = 3;
    [SerializeField] private int copperVeinMax = 8;

    [Header("Silver Veins")]
    [SerializeField] private int silverVeinMin = 2;
    [SerializeField] private int silverVeinMax = 6;

    [Header("Gold Veins")]
    [SerializeField] private int goldVeinMin = 2;
    [SerializeField] private int goldVeinMax = 5;

    [Header("Ruby Veins")]
    [SerializeField] private int rubyVeinMin = 1;
    [SerializeField] private int rubyVeinMax = 4;

    [Header("Diamond Veins")]
    [SerializeField] private int diamondVeinMin = 1;
    [SerializeField] private int diamondVeinMax = 3;

    // =========================================================
    // BLOCK HEALTH
    // =========================================================

    [Header("Block Health")]
    [SerializeField] private int stoneHealth = 3;
    [SerializeField] private int copperHealth = 4;
    [SerializeField] private int silverHealth = 6;
    [SerializeField] private int goldHealth = 8;
    [SerializeField] private int rubyHealth = 11;
    [SerializeField] private int diamondHealth = 15;

    // =========================================================
    // GOLD REWARDS
    // =========================================================

    [Header("Gold Rewards")]
    [SerializeField] private int copperGoldReward = 2;
    [SerializeField] private int silverGoldReward = 6;
    [SerializeField] private int goldGoldReward = 12;
    [SerializeField] private int rubyGoldReward = 30;
    [SerializeField] private int diamondGoldReward = 75;

    // =========================================================
    // XP REWARDS
    // =========================================================

    [Header("XP Rewards")]
    [SerializeField] private int stoneXP = 1;
    [SerializeField] private int copperXP = 2;
    [SerializeField] private int silverXP = 4;
    [SerializeField] private int goldXP = 7;
    [SerializeField] private int rubyXP = 12;
    [SerializeField] private int diamondXP = 20;

    // =========================================================
    // FLOOR SCALING
    // =========================================================

    [Header("Floor Scaling")]
    [SerializeField] private float healthPerFloor = 0.30f;
    [SerializeField] private float goldRewardPerFloor = 0.05f;
    [SerializeField] private float xpPerFloor = 0.07f;
    
    [Header("Background Variation")]
    [SerializeField] private float backgroundNoiseScale = 0.08f;
    [SerializeField] private float backgroundVariationStrength = 1f;

    // =========================================================
    // RANDOM
    // =========================================================

    [Header("Random Seed")]
    [SerializeField] private bool randomSeed = true;
    [SerializeField] private int seed = 12345;

    // =========================================================
    // PRIVATE
    // =========================================================

    private Vector3Int startCell;
    private GameObject entranceTorchInstance;
    private Vector3Int entranceTorchCell;
    private bool hasEntranceTorchCell;

    private readonly HashSet<Vector3Int> hiddenStairPositions =
        new HashSet<Vector3Int>();

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        GenerateMine();
    }

    // =========================================================
    // GENERATE MINE
    // =========================================================

    public void GenerateMine()
    {
        // -----------------------------------------------------
        // REFERENCES
        // -----------------------------------------------------

        if (backgroundTilemap == null)
        {
            Debug.LogError(
                "MineGenerator: Background Tilemap fehlt!"
            );

            return;
        }

        if (groundTilemap == null)
        {
            Debug.LogError(
                "MineGenerator: Ground Tilemap fehlt!"
            );

            return;
        }

        if (stairsTilemap == null)
        {
            Debug.LogError(
                "MineGenerator: Stairs Tilemap fehlt!"
            );

            return;
        }

        if (stoneTiles == null ||
            stoneTiles.Length == 0)
        {
            Debug.LogError(
                "MineGenerator: Keine Stone Tiles eingetragen!"
            );

            return;
        }

        if (caveBackgroundTiles == null ||
            caveBackgroundTiles.Length == 0)
        {
            Debug.LogError(
                "MineGenerator: Keine Background Tiles eingetragen!"
            );

            return;
        }

        bool validDecorationMap = backgroundDecorationTilemap != null &&
            backgroundDecorationTilemap != backgroundTilemap &&
            backgroundDecorationTilemap != groundTilemap &&
            backgroundDecorationTilemap != stairsTilemap;
        if (backgroundDecorationTilemap != null && !validDecorationMap)
            Debug.LogWarning("MineGenerator: Grass braucht eine eigene Decoration-Tilemap.", this);
        if (validDecorationMap)
            backgroundDecorationTilemap.ClearAllTiles();

        float backgroundNoiseOffsetX =
            Random.Range(
                -10000f,
                10000f
            );

        float backgroundNoiseOffsetY =
            Random.Range(
                -10000f,
                10000f
            );
        // -----------------------------------------------------
        // ALTE DATEN LÖSCHEN
        // -----------------------------------------------------

        if (playerMining != null)
        {
            playerMining.ClearBlockHealth();
        }

        if (mineFireflies == null)
            mineFireflies = GetComponent<MineFireflies>();
        if (mineFireflies != null)
            mineFireflies.ClearFireflies();

        ClearEntranceTorch();

        backgroundTilemap.ClearAllTiles();
        groundTilemap.ClearAllTiles();
        stairsTilemap.ClearAllTiles();

        if (stoneEdgeController != null)
        {
            stoneEdgeController.ClearAllEdges();
        }

        hiddenStairPositions.Clear();

        // -----------------------------------------------------
        // RANDOM SEED
        // -----------------------------------------------------

        if (randomSeed)
        {
            seed =
                Random.Range(
                    int.MinValue,
                    int.MaxValue
                );
        }

        Random.InitState(seed);

        // -----------------------------------------------------
        // BOUNDS
        // -----------------------------------------------------

        int minX =
            -(width / 2);

        int maxX =
            minX + width - 1;

        int topY =
            0;

        int bottomY =
            -(height - 1);

        // -----------------------------------------------------
        // START CELL
        // -----------------------------------------------------

        startCell =
            new Vector3Int(
                0,
                -borderThickness - 3,
                0
            );

        // -----------------------------------------------------
        // NOISE OFFSETS
        // -----------------------------------------------------

        float noiseOffsetX =
            Random.Range(
                -10000f,
                10000f
            );

        float noiseOffsetY =
            Random.Range(
                -10000f,
                10000f
            );

        // =====================================================
        // BACKGROUND + STONE + CAVES
        // =====================================================

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = bottomY; y <= topY; y++)
            {
                Vector3Int cell =
                    new Vector3Int(
                        x,
                        y,
                        0
                    );

                bool isBorder =
                    IsBorder(
                        x,
                        y,
                        minX,
                        maxX,
                        bottomY,
                        topY
                    );

                // =================================================
                // BACKGROUND
                // =================================================

                // Auf jeder normalen Minenzelle liegt ein
                // Höhlenboden unter Ground.
                if (!isBorder)
                {
                    TileBase backgroundTile =
                        GetBackgroundTile(
                            x,
                            y,
                            backgroundNoiseOffsetX,
                            backgroundNoiseOffsetY
                        );

                    if (backgroundTile != null)
                    {
                        backgroundTilemap.SetTile(
                            cell,
                            backgroundTile
                        );
                    }
                }

                // =================================================
                // BEDROCK BORDER
                // =================================================

                if (isBorder)
                {
                    groundTilemap.SetTile(
                        cell,
                        bedrockTile
                    );

                    continue;
                }

                // =================================================
                // START AREA
                // =================================================

                // Hier bleibt Ground frei.
                // Der Background darunter bleibt sichtbar.
                if (IsInsideStartArea(cell))
                {
                    continue;
                }

                // =================================================
                // CAVE NOISE
                // =================================================

                float noise =
                    Mathf.PerlinNoise(
                        x * caveScale +
                        noiseOffsetX,

                        y * caveScale +
                        noiseOffsetY
                    );

                // Cave = nur Background sichtbar.
                if (noise < caveThreshold)
                {
                    continue;
                }

                // =================================================
                // RANDOM STONE
                // =================================================

                groundTilemap.SetTile(
                    cell,
                    GetRandomStoneTile()
                );
            }
        }

        // =====================================================
        // ORE VEINS
        // =====================================================

        // Seltene zuerst, damit spätere Erze
        // sie nicht überschreiben.

        GenerateOreVeins(
            diamondOreTile,
            GetDiamondVeinCount(),
            diamondVeinMin,
            GetDiamondVeinMax()
        );

        GenerateOreVeins(
            rubyOreTile,
            GetRubyVeinCount(),
            rubyVeinMin,
            GetRubyVeinMax()
        );

        GenerateOreVeins(
            goldOreTile,
            GetGoldVeinCount(),
            goldVeinMin,
            GetGoldVeinMax()
        );

        GenerateOreVeins(
            silverOreTile,
            GetSilverVeinCount(),
            silverVeinMin,
            GetSilverVeinMax()
        );

        GenerateOreVeins(
            copperOreTile,
            GetCopperVeinCount(),
            copperVeinMin,
            GetCopperVeinMax()
        );

        // =====================================================
        // HIDDEN STAIRS
        // =====================================================

        GenerateHiddenStairs();

        // =====================================================
        // UP STAIRS
        // =====================================================

        PlaceUpStairs();
        PlaceEntranceTorch();

        if (validDecorationMap && generateGrass)
            GenerateGrassDecoration();

        // =====================================================
        // PLAYER
        // =====================================================

        if (mineFireflies != null && validDecorationMap && generateGrass)
            mineFireflies.Rebuild(backgroundDecorationTilemap, groundTilemap,
                backgroundTilemap, seed);

        PlacePlayer();

        // =====================================================
        // EDGES
        // =====================================================

        if (stoneEdgeController != null)
        {
            stoneEdgeController.RefreshAllEdges();
        }

        Debug.Log(
            "Mine Floor " +
            currentFloor +
            " generiert."
        );

        BuildingEffectManager.Instance?.OnFloorReady(currentFloor,
            player != null ? player.GetComponent<PlayerStats>() : null);
    }

    // =========================================================
    // RANDOM BACKGROUND
    // =========================================================

    // Grass uses its own random stream: ore/stair generation is unaffected.
    private void GenerateGrassDecoration()
    {
        var variants = new List<TileBase>();
        if (grassTiles != null)
            foreach (TileBase tile in grassTiles)
                if (tile != null) variants.Add(tile);
        if (variants.Count == 0)
        {
            Debug.LogWarning("MineGenerator: Keine Grass Tiles zugewiesen.", this);
            return;
        }

        var rng = new System.Random(unchecked(seed ^ 0x47A55));
        var candidates = new List<Vector3Int>();
        var eligible = new HashSet<Vector3Int>();
        int minX = -(width / 2);
        for (int x = minX; x < minX + width; x++)
            for (int y = -(height - 1); y <= 0; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (!CanPlaceGrass(cell)) continue;
                candidates.Add(cell);
                eligible.Add(cell);
            }

        // Shuffle so clusters are not biased toward one side of the map.
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            Vector3Int temp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = temp;
        }

        var blocked = new HashSet<Vector3Int>();
        Vector3Int[] directions = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };
        int minSize = Mathf.Clamp(grassClusterMinSize, 1, 100);
        int maxSize = Mathf.Clamp(grassClusterMaxSize, minSize, 100);
        int spacing = Mathf.Clamp(grassClusterSpacing, 0, 3);

        foreach (Vector3Int origin in candidates)
        {
            if (blocked.Contains(origin) ||
                rng.NextDouble() >= Mathf.Clamp01(grassClusterChance)) continue;

            int targetSize = rng.Next(minSize, maxSize + 1);
            var frontier = new List<Vector3Int> { origin };
            var visited = new HashSet<Vector3Int> { origin };
            var cluster = new List<Vector3Int>();
            while (frontier.Count > 0 && cluster.Count < targetSize)
            {
                int index = rng.Next(frontier.Count);
                Vector3Int cell = frontier[index];
                frontier.RemoveAt(index);
                cluster.Add(cell);
                foreach (Vector3Int direction in directions)
                {
                    Vector3Int next = cell + direction;
                    if (eligible.Contains(next) && !blocked.Contains(next) && visited.Add(next))
                        frontier.Add(next);
                }
            }

            foreach (Vector3Int cell in cluster)
            {
                backgroundDecorationTilemap.SetTile(cell, variants[rng.Next(variants.Count)]);
                for (int dx = -spacing; dx <= spacing; dx++)
                    for (int dy = -spacing; dy <= spacing; dy++)
                        blocked.Add(cell + new Vector3Int(dx, dy, 0));
            }
        }
    }

    private bool CanPlaceGrass(Vector3Int cell)
    {
        if (hasEntranceTorchCell && cell == entranceTorchCell)
            return false;

        if (!backgroundTilemap.HasTile(cell) || groundTilemap.HasTile(cell) ||
            stairsTilemap.HasTile(cell) || hiddenStairPositions.Contains(cell) ||
            cell == startCell || cell == startCell + Vector3Int.up)
            return false;

        int clearance = Mathf.Clamp(grassDistanceFromRock, 0, 4);
        for (int dx = -clearance; dx <= clearance; dx++)
            for (int dy = -clearance; dy <= clearance; dy++)
            {
                Vector3Int neighbor = cell + new Vector3Int(dx, dy, 0);
                if (!backgroundTilemap.HasTile(neighbor) || groundTilemap.HasTile(neighbor))
                    return false;
            }
        return true;
    }

    private TileBase GetBackgroundTile(
        int x,
        int y,
        float offsetX,
        float offsetY)
    {
        if (caveBackgroundTiles == null ||
            caveBackgroundTiles.Length == 0)
        {
            return null;
        }

        if (caveBackgroundTiles.Length == 1)
        {
            return caveBackgroundTiles[0];
        }

        float noise =
            Mathf.PerlinNoise(
                x * backgroundNoiseScale + offsetX,
                y * backgroundNoiseScale + offsetY
            );

        noise *= backgroundVariationStrength;

        noise = Mathf.Clamp01(noise);

        int index =
            Mathf.FloorToInt(
                noise *
                caveBackgroundTiles.Length
            );

        index =
            Mathf.Clamp(
                index,
                0,
                caveBackgroundTiles.Length - 1
            );

        return caveBackgroundTiles[index];
    }

    // =========================================================
    // RANDOM STONE
    // =========================================================

    private TileBase GetRandomStoneTile()
    {
        if (stoneTiles == null ||
            stoneTiles.Length == 0)
        {
            return null;
        }

        return stoneTiles[
            Random.Range(
                0,
                stoneTiles.Length
            )
        ];
    }

    // =========================================================
    // TILE CHECKS
    // =========================================================

    public bool IsStoneTile(
        TileBase tile)
    {
        if (tile == null ||
            stoneTiles == null)
        {
            return false;
        }

        foreach (TileBase stone in stoneTiles)
        {
            if (tile == stone)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsOreTile(
        TileBase tile)
    {
        return
            tile == copperOreTile ||
            tile == silverOreTile ||
            tile == goldOreTile ||
            tile == rubyOreTile ||
            tile == diamondOreTile;
    }

    // =========================================================
    // ORE VEINS
    // =========================================================

    private void GenerateOreVeins(
        TileBase oreTile,
        int veinCount,
        int minimumVeinSize,
        int maximumVeinSize)
    {
        if (oreTile == null)
            return;

        if (veinCount <= 0)
            return;

        for (int i = 0; i < veinCount; i++)
        {
            if (!TryFindStonePosition(
                out Vector3Int startPosition))
            {
                continue;
            }

            int veinSize =
                Random.Range(
                    minimumVeinSize,
                    maximumVeinSize + 1
                );

            CreateOreVein(
                startPosition,
                oreTile,
                veinSize
            );
        }
    }

    // =========================================================
    // CREATE ORE VEIN
    // =========================================================

    private void CreateOreVein(
        Vector3Int startPosition,
        TileBase oreTile,
        int veinSize)
    {
        List<Vector3Int> placedPositions =
            new List<Vector3Int>();

        if (!IsStoneTile(
            groundTilemap.GetTile(
                startPosition)))
        {
            return;
        }

        groundTilemap.SetTile(
            startPosition,
            oreTile
        );

        placedPositions.Add(
            startPosition
        );

        int attempts =
            veinSize * 8;

        while (
            placedPositions.Count < veinSize &&
            attempts > 0)
        {
            attempts--;

            Vector3Int origin =
                placedPositions[
                    Random.Range(
                        0,
                        placedPositions.Count
                    )
                ];

            Vector3Int next =
                origin +
                GetRandomCardinalDirection();

            TileBase nextTile =
                groundTilemap.GetTile(
                    next
                );

            if (!IsStoneTile(nextTile))
            {
                continue;
            }

            groundTilemap.SetTile(
                next,
                oreTile
            );

            placedPositions.Add(
                next
            );
        }
    }

    // =========================================================
    // FIND STONE POSITION
    // =========================================================

    private bool TryFindStonePosition(
        out Vector3Int position)
    {
        int minX =
            -(width / 2);

        int maxX =
            minX + width - 1;

        int topY =
            0;

        int bottomY =
            -(height - 1);

        const int maxAttempts =
            150;

        for (int i = 0; i < maxAttempts; i++)
        {
            int x =
                Random.Range(
                    minX + borderThickness,
                    maxX - borderThickness + 1
                );

            int y =
                Random.Range(
                    bottomY + borderThickness,
                    topY - borderThickness + 1
                );

            Vector3Int test =
                new Vector3Int(
                    x,
                    y,
                    0
                );

            if (IsInsideStartArea(test))
                continue;

            TileBase tile =
                groundTilemap.GetTile(
                    test
                );

            if (!IsStoneTile(tile))
                continue;

            position = test;

            return true;
        }

        position =
            Vector3Int.zero;

        return false;
    }

    // =========================================================
    // RANDOM DIRECTION
    // =========================================================

    private Vector3Int GetRandomCardinalDirection()
    {
        switch (Random.Range(0, 4))
        {
            case 0:
                return Vector3Int.up;

            case 1:
                return Vector3Int.down;

            case 2:
                return Vector3Int.left;

            default:
                return Vector3Int.right;
        }
    }

    // =========================================================
    // ORE COUNTS
    // =========================================================

    private int GetCopperVeinCount()
    {
        if (currentFloor < copperUnlockFloor)
            return 0;

        return Mathf.Clamp(
            14 -
            Mathf.FloorToInt(
                (currentFloor - 1) *
                0.55f
            ),
            2,
            14
        );
    }

    private int GetSilverVeinCount()
    {
        if (currentFloor < silverUnlockFloor)
            return 0;

        int count =
            4 +
            Mathf.FloorToInt(
                (currentFloor -
                 silverUnlockFloor) *
                0.55f
            );

        if (currentFloor > 15)
        {
            count -=
                Mathf.FloorToInt(
                    (currentFloor - 15) *
                    0.35f
                );
        }

        return Mathf.Clamp(
            count,
            2,
            11
        );
    }

    private int GetGoldVeinCount()
    {
        if (currentFloor < goldUnlockFloor)
            return 0;

        int count =
            3 +
            Mathf.FloorToInt(
                (currentFloor -
                 goldUnlockFloor) *
                0.45f
            );

        if (currentFloor > 25)
        {
            count -=
                Mathf.FloorToInt(
                    (currentFloor - 25) *
                    0.25f
                );
        }

        return Mathf.Clamp(
            count,
            2,
            10
        );
    }

    private int GetRubyVeinCount()
    {
        if (currentFloor < rubyUnlockFloor)
            return 0;

        return Mathf.Clamp(
            2 +
            Mathf.FloorToInt(
                (currentFloor -
                 rubyUnlockFloor) *
                0.35f
            ),
            2,
            9
        );
    }

    private int GetDiamondVeinCount()
    {
        if (currentFloor < diamondUnlockFloor)
            return 0;

        return Mathf.Clamp(
            1 +
            Mathf.FloorToInt(
                (currentFloor -
                 diamondUnlockFloor) *
                0.20f
            ),
            1,
            6
        );
    }

    // =========================================================
    // VEIN MAX
    // =========================================================

    private int GetCopperVeinMax()
    {
        return copperVeinMax +
            Mathf.Clamp(
                Mathf.FloorToInt(
                    (currentFloor - 1) /
                    10f
                ),
                0,
                3
            );
    }

    private int GetSilverVeinMax()
    {
        int depth =
            Mathf.Max(
                0,
                currentFloor -
                silverUnlockFloor
            );

        return silverVeinMax +
            Mathf.Clamp(
                Mathf.FloorToInt(
                    depth /
                    12f
                ),
                0,
                3
            );
    }

    private int GetGoldVeinMax()
    {
        int depth =
            Mathf.Max(
                0,
                currentFloor -
                goldUnlockFloor
            );

        return goldVeinMax +
            Mathf.Clamp(
                Mathf.FloorToInt(
                    depth /
                    15f
                ),
                0,
                3
            );
    }

    private int GetRubyVeinMax()
    {
        int depth =
            Mathf.Max(
                0,
                currentFloor -
                rubyUnlockFloor
            );

        return rubyVeinMax +
            Mathf.Clamp(
                Mathf.FloorToInt(
                    depth /
                    15f
                ),
                0,
                2
            );
    }

    private int GetDiamondVeinMax()
    {
        int depth =
            Mathf.Max(
                0,
                currentFloor -
                diamondUnlockFloor
            );

        return diamondVeinMax +
            Mathf.Clamp(
                Mathf.FloorToInt(
                    depth /
                    15f
                ),
                0,
                3
            );
    }

    // =========================================================
    // HIDDEN STAIRS
    // =========================================================

    private void GenerateHiddenStairs()
    {
        hiddenStairPositions.Clear();

        int attempts = 0;

        int maxAttempts =
            stairsPerFloor *
            300;

        int minX =
            -(width / 2);

        int maxX =
            minX + width - 1;

        int topY =
            0;

        int bottomY =
            -(height - 1);

        while (
            hiddenStairPositions.Count <
            stairsPerFloor &&
            attempts < maxAttempts)
        {
            attempts++;

            Vector3Int cell =
                new Vector3Int(
                    Random.Range(
                        minX + borderThickness,
                        maxX - borderThickness + 1
                    ),
                    Random.Range(
                        bottomY + borderThickness,
                        topY - borderThickness + 1
                    ),
                    0
                );

            if (IsInsideStartArea(cell))
                continue;

            TileBase tile =
                groundTilemap.GetTile(
                    cell
                );

            if (tile == null)
                continue;

            if (tile == bedrockTile)
                continue;

            hiddenStairPositions.Add(
                cell
            );
        }
    }

    // =========================================================
    // HIDDEN STAIRS API
    // =========================================================

    public bool HasHiddenStairs(
        Vector3Int cell)
    {
        return hiddenStairPositions.Contains(
            cell
        );
    }

    public void RevealStairs(
        Vector3Int cell)
    {
        if (!hiddenStairPositions.Contains(
            cell))
        {
            return;
        }

        hiddenStairPositions.Remove(
            cell
        );

        if (stairsDownTile != null)
        {
            stairsTilemap.SetTile(
                cell,
                stairsDownTile
            );
        }
    }

    public IEnumerable<Vector3Int>
        GetHiddenStairPositions()
    {
        return hiddenStairPositions;
    }

    // =========================================================
    // UP STAIRS
    // =========================================================

    private void ClearEntranceTorch()
    {
        hasEntranceTorchCell = false;
        if (entranceTorchInstance == null)
            return;

        // Disable the old light immediately; Destroy completes at the end of the frame.
        entranceTorchInstance.SetActive(false);
        Destroy(entranceTorchInstance);
        entranceTorchInstance = null;
    }

    private void PlaceEntranceTorch()
    {
        if (!spawnEntranceTorch || entranceTorchPrefab == null || stairsUpTile == null)
            return;

        Vector3Int stairsCell = startCell + Vector3Int.up;
        Vector3Int preferred = new Vector3Int(
            entranceTorchCellOffset.x, entranceTorchCellOffset.y, 0);
        Vector3Int[] offsets =
        {
            preferred, Vector3Int.right, Vector3Int.left,
            Vector3Int.up, Vector3Int.down
        };

        foreach (Vector3Int offset in offsets)
        {
            Vector3Int cell = stairsCell + offset;
            if (cell == startCell || cell == stairsCell ||
                groundTilemap.HasTile(cell) || stairsTilemap.HasTile(cell) ||
                !backgroundTilemap.HasTile(cell))
                continue;

            Vector3 position = groundTilemap.GetCellCenterWorld(cell) +
                entranceTorchWorldOffset;
            entranceTorchInstance = Instantiate(
                entranceTorchPrefab, position, entranceTorchPrefab.transform.rotation,
                transform);
            entranceTorchInstance.name = "EntranceTorch";
            entranceTorchInstance.SetActive(true);
            entranceTorchCell = cell;
            hasEntranceTorchCell = true;
            return;
        }

        Debug.LogWarning("MineGenerator: No free cell beside the entrance for a torch.", this);
    }

    private void PlaceUpStairs()
    {
        if (stairsUpTile == null)
            return;

        Vector3Int stairsUpCell =
            startCell +
            Vector3Int.up;

        stairsTilemap.SetTile(
            stairsUpCell,
            stairsUpTile
        );
    }

    // =========================================================
    // PLAYER
    // =========================================================

    private void PlacePlayer()
    {
        if (player == null)
            return;

        Vector3 position =
            groundTilemap.GetCellCenterWorld(
                startCell
            );

        position.z =
            player.position.z;

        player.position =
            position;
    }

    // =========================================================
    // NEXT FLOOR
    // =========================================================

    public void GoToNextFloor()
    {
        // Cartographer Preview entfernen.
        if (cartographersEyeEffect != null)
        {
            cartographersEyeEffect.OnFloorChanged();
        }

        // Alte Edges entfernen.
        if (stoneEdgeController != null)
        {
            stoneEdgeController.ClearAllEdges();
        }

        currentFloor++;

        GenerateMine();
    }

    // =========================================================
    // BLOCK HEALTH
    // =========================================================

    public int GetBlockHealth(
        TileBase tile)
    {
        int baseHealth =
            stoneHealth;

        if (tile == copperOreTile)
            baseHealth = copperHealth;
        else if (tile == silverOreTile)
            baseHealth = silverHealth;
        else if (tile == goldOreTile)
            baseHealth = goldHealth;
        else if (tile == rubyOreTile)
            baseHealth = rubyHealth;
        else if (tile == diamondOreTile)
            baseHealth = diamondHealth;
        else if (IsStoneTile(tile))
            baseHealth = stoneHealth;

        int floorBonus =
            Mathf.FloorToInt(
                (currentFloor - 1) *
                healthPerFloor
            );

        return Mathf.Max(
            1,
            baseHealth +
            floorBonus
        );
    }

    // =========================================================
    // GOLD REWARD
    // =========================================================

    public int GetGoldReward(
        TileBase tile)
    {
        int reward = 0;

        if (tile == copperOreTile)
            reward = copperGoldReward;
        else if (tile == silverOreTile)
            reward = silverGoldReward;
        else if (tile == goldOreTile)
            reward = goldGoldReward;
        else if (tile == rubyOreTile)
            reward = rubyGoldReward;
        else if (tile == diamondOreTile)
            reward = diamondGoldReward;

        if (reward <= 0)
            return 0;

        float multiplier =
            1f +
            (currentFloor - 1) *
            goldRewardPerFloor;

        return Mathf.Max(
            1,
            Mathf.RoundToInt(
                reward *
                multiplier
            )
        );
    }

    // =========================================================
    // XP REWARD
    // =========================================================

    public int GetXPReward(
        TileBase tile)
    {
        int xp = 0;

        if (IsStoneTile(tile))
            xp = stoneXP;
        else if (tile == copperOreTile)
            xp = copperXP;
        else if (tile == silverOreTile)
            xp = silverXP;
        else if (tile == goldOreTile)
            xp = goldXP;
        else if (tile == rubyOreTile)
            xp = rubyXP;
        else if (tile == diamondOreTile)
            xp = diamondXP;

        if (xp <= 0)
            return 0;

        float multiplier =
            1f +
            (currentFloor - 1) *
            xpPerFloor;

        return Mathf.Max(
            1,
            Mathf.RoundToInt(
                xp *
                multiplier
            )
        );
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private bool IsInsideStartArea(
        Vector3Int cell)
    {
        int distanceX =
            Mathf.Abs(
                cell.x -
                startCell.x
            );

        int distanceY =
            Mathf.Abs(
                cell.y -
                startCell.y
            );

        return
            distanceX <=
            startAreaRadius &&
            distanceY <=
            startAreaRadius;
    }

    private bool IsBorder(
        int x,
        int y,
        int minX,
        int maxX,
        int bottomY,
        int topY)
    {
        return
            x < minX + borderThickness ||
            x > maxX - borderThickness ||
            y < bottomY + borderThickness ||
            y > topY - borderThickness;
    }
}
