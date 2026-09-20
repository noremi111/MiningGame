using UnityEngine;
using UnityEngine.Tilemaps;

public class StoneEdgeController : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private MineGenerator mineGenerator;

    // =========================================================
    // EDGE TILEMAPS
    // =========================================================

    [Header("Edge Tilemaps")]
    [SerializeField] private Tilemap edgeTopTilemap;
    [SerializeField] private Tilemap edgeBottomTilemap;
    [SerializeField] private Tilemap edgeLeftTilemap;
    [SerializeField] private Tilemap edgeRightTilemap;

    // =========================================================
    // EDGE TILES
    // =========================================================

    [Header("Edge Tiles")]
    [SerializeField] private TileBase edgeTopTile;
    [SerializeField] private TileBase edgeBottomTile;
    [SerializeField] private TileBase edgeLeftTile;
    [SerializeField] private TileBase edgeRightTile;

    // =========================================================
    // MINE SIZE
    // =========================================================

    [Header("Mine Size")]
    [SerializeField] private int width = 60;
    [SerializeField] private int height = 60;

    // =========================================================
    // REFRESH ALL
    // =========================================================

    public void RefreshAllEdges()
    {
        if (!ReferencesValid())
            return;

        ClearAllEdges();

        int minX = -(width / 2);
        int maxX = minX + width - 1;

        int topY = 0;
        int bottomY = -(height - 1);

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

                RefreshCell(cell);
            }
        }
    }

    // =========================================================
    // REFRESH LOCAL AREA
    // =========================================================

    public void RefreshAround(Vector3Int center)
    {
        if (!ReferencesValid())
            return;

        // -----------------------------------------------------
        // 1. ALTE EDGES IM 3x3-BEREICH KOMPLETT LÖSCHEN
        // -----------------------------------------------------

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int cell =
                    center +
                    new Vector3Int(
                        x,
                        y,
                        0
                    );

                ClearCellEdges(cell);
            }
        }

        // -----------------------------------------------------
        // 2. 3x3-BEREICH KOMPLETT NEU BERECHNEN
        // -----------------------------------------------------

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int cell =
                    center +
                    new Vector3Int(
                        x,
                        y,
                        0
                    );

                RefreshCell(cell);
            }
        }
    }

    // =========================================================
    // REFRESH ONE CELL
    // =========================================================

    private void RefreshCell(Vector3Int cell)
    {
        if (!ReferencesValid())
            return;

        ClearCellEdges(cell);

        TileBase currentTile =
            groundTilemap.GetTile(
                cell
            );

        if (!IsSolidRock(currentTile))
            return;

        bool openTop =
            !IsSolidRock(
                groundTilemap.GetTile(
                    cell + Vector3Int.up
                )
            );

        bool openBottom =
            !IsSolidRock(
                groundTilemap.GetTile(
                    cell + Vector3Int.down
                )
            );

        bool openLeft =
            !IsSolidRock(
                groundTilemap.GetTile(
                    cell + Vector3Int.left
                )
            );

        bool openRight =
            !IsSolidRock(
                groundTilemap.GetTile(
                    cell + Vector3Int.right
                )
            );

        // =====================================================
        // JEDE RICHTUNG UNABHÄNGIG SETZEN
        // =====================================================

        if (openTop &&
            edgeTopTile != null)
        {
            edgeTopTilemap.SetTile(
                cell,
                edgeTopTile
            );
        }

        if (openBottom &&
            edgeBottomTile != null)
        {
            edgeBottomTilemap.SetTile(
                cell,
                edgeBottomTile
            );
        }

        if (openLeft &&
            edgeLeftTile != null)
        {
            edgeLeftTilemap.SetTile(
                cell,
                edgeLeftTile
            );
        }

        if (openRight &&
            edgeRightTile != null)
        {
            edgeRightTilemap.SetTile(
                cell,
                edgeRightTile
            );
        }
    }

    // =========================================================
    // CLEAR ONE CELL
    // =========================================================

    private void ClearCellEdges(
        Vector3Int cell)
    {
        if (edgeTopTilemap != null)
        {
            edgeTopTilemap.SetTile(
                cell,
                null
            );
        }

        if (edgeBottomTilemap != null)
        {
            edgeBottomTilemap.SetTile(
                cell,
                null
            );
        }

        if (edgeLeftTilemap != null)
        {
            edgeLeftTilemap.SetTile(
                cell,
                null
            );
        }

        if (edgeRightTilemap != null)
        {
            edgeRightTilemap.SetTile(
                cell,
                null
            );
        }
    }

    // =========================================================
    // CLEAR ALL
    // =========================================================

    public void ClearAllEdges()
    {
        if (edgeTopTilemap != null)
            edgeTopTilemap.ClearAllTiles();

        if (edgeBottomTilemap != null)
            edgeBottomTilemap.ClearAllTiles();

        if (edgeLeftTilemap != null)
            edgeLeftTilemap.ClearAllTiles();

        if (edgeRightTilemap != null)
            edgeRightTilemap.ClearAllTiles();
    }

    // =========================================================
    // SOLID ROCK CHECK
    // =========================================================

    private bool IsSolidRock(
        TileBase tile)
    {
        if (tile == null)
            return false;

        if (mineGenerator == null)
            return false;

        if (mineGenerator.IsStoneTile(tile))
            return true;

        if (mineGenerator.IsOreTile(tile))
            return true;

        return false;
    }

    // =========================================================
    // VALIDATION
    // =========================================================

    private bool ReferencesValid()
    {
        if (groundTilemap == null)
        {
            Debug.LogWarning(
                "StoneEdgeController: Ground Tilemap fehlt."
            );

            return false;
        }

        if (mineGenerator == null)
        {
            Debug.LogWarning(
                "StoneEdgeController: MineGenerator fehlt."
            );

            return false;
        }

        if (edgeTopTilemap == null ||
            edgeBottomTilemap == null ||
            edgeLeftTilemap == null ||
            edgeRightTilemap == null)
        {
            Debug.LogWarning(
                "StoneEdgeController: Mindestens eine Edge-Tilemap fehlt."
            );

            return false;
        }

        return true;
    }
}