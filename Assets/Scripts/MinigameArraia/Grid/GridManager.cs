using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum CellType { Empty, Wall, Box, Target, BoxOnTarget }

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Tilemaps de referência")]
    public Tilemap wallsTilemap;
    public Tilemap targetsTilemap;
    public Tilemap spawnsTilemap;

    [Header("Tiles de marcação (spawn)")]
    public TileBase playerSpawnTile;
    public TileBase boxSpawnTile;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject boxPrefab;

    private Dictionary<Vector2Int, CellType> grid = new();

    private void Awake()
    {
        Instance = this;
        BuildFromTilemaps();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void BuildFromTilemaps()
    {
        grid.Clear();
        BoundsInt bounds = wallsTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                Vector2Int gridPos = new Vector2Int(x, y);
                if (wallsTilemap.HasTile(cellPos))
                {
                    SetCell(gridPos, CellType.Wall);
                }
                else if (targetsTilemap != null && targetsTilemap.HasTile(cellPos))
                {
                    SetCell(gridPos, CellType.Target);
                }
                else
                {
                    SetCell(gridPos, CellType.Empty);
                }
            }
        }

        SpawnEntitiesFromTilemap();
    }

    private void SpawnEntitiesFromTilemap()
    {
        if (spawnsTilemap == null) return;

        BoundsInt bounds = spawnsTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase tile = spawnsTilemap.GetTile(cellPos);

                if (tile == null) continue; // célula vazia, não tem marcação

                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = GridToWorld(gridPos);

                if (tile == playerSpawnTile)
                {
                    Instantiate(playerPrefab, worldPos, Quaternion.identity);
                }
                else if (tile == boxSpawnTile)
                {
                    Instantiate(boxPrefab, worldPos, Quaternion.identity);
                    SetCell(gridPos, CellType.Box);
                }
            }
        }
        spawnsTilemap.gameObject.SetActive(false);
    }

    public CellType GetCell(Vector2Int pos)
    {
        if (grid.TryGetValue(pos, out CellType type))
            return type;
        return CellType.Wall;
    }

    public void SetCell(Vector2Int pos, CellType type)
    {
        grid[pos] = type;
    }

    public bool IsWalkable(Vector2Int pos)
    {
        CellType type = GetCell(pos);
        return type == CellType.Empty || type == CellType.Target;
    }

    public bool IsBox(Vector2Int pos)
    {
        CellType type = GetCell(pos);
        return type == CellType.Box || type == CellType.BoxOnTarget;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3Int cell = wallsTilemap.WorldToCell(worldPos);
        return new Vector2Int(cell.x, cell.y);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        Vector3Int cell = new Vector3Int(gridPos.x, gridPos.y, 0);
        return wallsTilemap.GetCellCenterWorld(cell);
    }
}