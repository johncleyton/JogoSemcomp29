using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSpawner : MonoBehaviour
{
    public enum FitTarget { Ground, Frame }

    public List<GameObject> prefabs = new List<GameObject>();

    [Header("Moldura")]
    [Tooltip("Prefab com SpriteRenderer. O sprite precisa ter Mesh Type = Full Rect")]
    public SpriteRenderer framePrefab;
    [Tooltip("Folga entre o ch�o e a moldura (unidades de mundo, antes do resize)")]
    public float innerPadding = 0.1f;
    public int frameSortingOrder = -100;

    [Header("Enquadramento na c�mera")]
    public Camera targetCamera;
    [Tooltip("Ground = a �rea jog�vel ocupa o m�ximo poss�vel (moldura pode sair da tela). Frame = a moldura inteira cabe na tela.")]
    public FitTarget fitTarget = FitTarget.Ground;
    [Range(0f, 0.4f)] public float marginX = 0.02f;
    [Range(0f, 0.4f)] public float marginY = 0.05f;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        int index = Random.Range(0, prefabs.Count);
        GameObject level = Instantiate(prefabs[index]);

        GridManager grid = level.GetComponentInChildren<GridManager>();
        if (grid == null)
        {
            Debug.LogWarning("GridSpawner: fase sem GridManager.");
            return;
        }

        Tilemap area = grid.groundTilemap != null ? grid.groundTilemap : grid.wallsTilemap;
        if (area == null)
        {
            Debug.LogWarning("GridSpawner: fase sem groundTilemap/wallsTilemap.");
            return;
        }

        TilemapRenderer areaRenderer = area.GetComponent<TilemapRenderer>();

        // 1) Bounds da �rea jog�vel (escala original)
        area.CompressBounds();
        Bounds inner = areaRenderer.bounds;

        // 2) Moldura em volta, filha da fase (escala junto)
        SpriteRenderer frame = null;
        if (framePrefab != null)
        {
            frame = Instantiate(framePrefab, level.transform);
            frame.drawMode = SpriteDrawMode.Tiled;
            frame.tileMode = SpriteTileMode.Continuous;
            frame.sortingOrder = frameSortingOrder;

            Sprite s = frame.sprite;
            Vector4 b = s.border / s.pixelsPerUnit; // x=L, y=B, z=R, w=T

            frame.size = new Vector2(
                inner.size.x + innerPadding * 2f + b.x + b.z,
                inner.size.y + innerPadding * 2f + b.y + b.w
            );
            frame.transform.position = new Vector3(
                inner.center.x + (b.z - b.x) * 0.5f,
                inner.center.y + (b.w - b.y) * 0.5f,
                frame.transform.position.z
            );
        }

        // 3) Escala a fase pra caber na câmera
        bool useFrame = fitTarget == FitTarget.Frame && frame != null;
        Bounds fitBounds = useFrame ? frame.bounds : inner;

        float viewH = targetCamera.orthographicSize * 2f;
        float viewW = viewH * targetCamera.aspect;
        float availW = viewW * (1f - marginX * 2f);
        float availH = viewH * (1f - marginY * 2f);

        float scale = Mathf.Min(availW / fitBounds.size.x, availH / fitBounds.size.y);

        Vector3 localCenter = level.transform.InverseTransformPoint(fitBounds.center);

        // Aplica a escala (os bounds do Unity não vão atualizar a tempo para a próxima linha)
        level.transform.localScale *= scale;

        // 4) Centraliza na câmera usando cálculo de matriz direto, ignorando bounds defasados
        Vector3 newWorldCenter = level.transform.TransformPoint(localCenter);
        Vector3 camCenter = targetCamera.transform.position;
        
        level.transform.position += new Vector3(
            camCenter.x - newWorldCenter.x,
            camCenter.y - newWorldCenter.y,
            0f
        );
    }
}