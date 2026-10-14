using UnityEngine;
using System.Collections;

public class BoxController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public Vector2Int GridPos { get; private set; }

    private Camera cam;
    private Vector2 dragStart;
    private bool isDragging = false;
    private const float minSwipeDistance = 0.3f; // em unidades da fase original (escala 1)

    private void Start()
    {
        cam = Camera.main;
        GridPos = GridManager.Instance.WorldToGrid(transform.position);
        GridManager.Instance.RegisterBox(GridPos, this);
    }

    private void OnMouseDown()
    {
        dragStart = cam.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        Vector2 dragEnd = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 delta = dragEnd - dragStart;

        if (delta.magnitude < minSwipeDistance * GridManager.Instance.WorldScale) return;

        TryMove(GetDominantDirection(delta));
    }

    private Vector2Int GetDominantDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int targetPos = GridPos + direction;

        if (!GridManager.Instance.IsWalkable(targetPos) || GridManager.Instance.IsBox(targetPos)){
            GridManager.Instance.BalaoPop();
        }
            

        MoveTo(targetPos);
    }

    public void MoveTo(Vector2Int newGridPos)
    {
        GridManager.Instance.UnregisterBox(GridPos);

        CellType cellUnderBox = GridManager.Instance.GetCell(GridPos);
        GridManager.Instance.SetCell(GridPos, cellUnderBox == CellType.BoxOnTarget ? CellType.Target : CellType.Empty);

        GridPos = newGridPos;
        GridManager.Instance.RegisterBox(GridPos, this);

        CellType cellAtDestination = GridManager.Instance.GetCell(GridPos);
        GridManager.Instance.SetCell(GridPos, cellAtDestination == CellType.Target ? CellType.BoxOnTarget : CellType.Box);

        StartCoroutine(MoveRoutine(GridManager.Instance.GridToWorld(GridPos)));
        GridManager.Instance.CheckWinCondition();
    }

    private IEnumerator MoveRoutine(Vector3 targetWorldPos)
    {
        float scale = GridManager.Instance.WorldScale;

        while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f * scale)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * scale * Time.deltaTime);
            yield return null;
        }
        transform.position = targetWorldPos;
    }
}