using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum CellType { Empty, Wall, Box, Target, BoxOnTarget }

public class GridManager : MinigameBase
{
    public static GridManager Instance;

    [Header("Tilemaps de referência")]
    [Tooltip("Define a área jogável. Só células com tile aqui existem.")]
    public Tilemap groundTilemap;
    [Tooltip("Obstáculos (plantas)")]
    public Tilemap wallsTilemap;
    public Tilemap targetsTilemap;
    public Tilemap spawnsTilemap;

    [Header("Tiles de marcação (spawn)")]
    public TileBase boxSpawnTile;

    [Header("Prefabs")]
    public GameObject boxPrefab;

    private Dictionary<Vector2Int, CellType> grid = new();

    private Dictionary<Vector2Int, BoxController> boxLookup = new();
    public void RegisterBox(Vector2Int pos, BoxController box) => boxLookup[pos] = box;
    public void UnregisterBox(Vector2Int pos) => boxLookup.Remove(pos);
    public BoxController GetBoxAt(Vector2Int pos) => boxLookup.TryGetValue(pos, out var b) ? b : null;

    // Tilemap usado como referência de coordenadas (todos devem estar no mesmo Grid)
    private Tilemap RefTilemap => groundTilemap != null ? groundTilemap : wallsTilemap;

    // Fator de escala aplicado à fase (1 = tamanho original)
    public float WorldScale => RefTilemap != null ? RefTilemap.transform.lossyScale.x : 1f;

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

        // Se não houver ground, cai no comportamento antigo (bounds das paredes)
        Tilemap area = groundTilemap != null ? groundTilemap : wallsTilemap;
        BoundsInt bounds = area.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                Vector2Int gridPos = new Vector2Int(x, y);

                // Sem chão = célula não existe (GetCell retorna Wall)
                if (groundTilemap != null && !groundTilemap.HasTile(cellPos))
                    continue;

                if (wallsTilemap != null && wallsTilemap.HasTile(cellPos))
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

                if (tile == null) continue;

                Vector2Int gridPos = new Vector2Int(x, y);

                if (tile == boxSpawnTile)
                {
                    Vector3 worldPos = GridToWorld(gridPos);
                    Instantiate(boxPrefab, worldPos, Quaternion.identity, transform);
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

    public void CheckWinCondition()
    {
        foreach (var kvp in grid)
        {
            if (kvp.Value == CellType.Box)
            {
                return;
            }
        }
        Vencer();
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
        Vector3Int cell = RefTilemap.WorldToCell(worldPos);
        return new Vector2Int(cell.x, cell.y);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        Vector3Int cell = new Vector3Int(gridPos.x, gridPos.y, 0);
        return RefTilemap.GetCellCenterWorld(cell);
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        return 30f;
    }
}