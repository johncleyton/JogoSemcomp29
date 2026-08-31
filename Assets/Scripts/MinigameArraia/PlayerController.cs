using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 8f;

    private Vector2Int gridPos;
    private bool isMoving = false;

    private void Start()
    {
        gridPos = GridManager.Instance.WorldToGrid(transform.position);
    }

    private void Update()
    {
        if (isMoving) return;

        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            direction = Vector2Int.right;

        if (direction != Vector2Int.zero)
        {
            TryMove(direction);
        }
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int targetPos = gridPos + direction;

        if (GridManager.Instance.IsBox(targetPos))
        {
            Vector2Int boxTargetPos = targetPos + direction;

            if (GridManager.Instance.IsWalkable(boxTargetPos) && !GridManager.Instance.IsBox(boxTargetPos))
            {
                PushBox(targetPos, boxTargetPos);
                MoveTo(targetPos);
            }
            return;
        }

        if (GridManager.Instance.IsWalkable(targetPos))
        {
            MoveTo(targetPos);
        }
    }

    private void MoveTo(Vector2Int newPos)
    {
        gridPos = newPos;
        Vector3 worldPos = GridManager.Instance.GridToWorld(newPos);
        StartCoroutine(MoveRoutine(worldPos));
    }

    private IEnumerator MoveRoutine(Vector3 targetWorldPos)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetWorldPos;
        isMoving = false;
    }

    private void PushBox(Vector2Int boxCurrentPos, Vector2Int boxNewPos)
    {
        BoxController box = FindBoxAt(boxCurrentPos);
        if (box != null)
        {
            box.MoveTo(boxNewPos);
        }

        CellType cellUnderBox = GridManager.Instance.GetCell(boxCurrentPos);
        GridManager.Instance.SetCell(
            boxCurrentPos,
            cellUnderBox == CellType.BoxOnTarget ? CellType.Target : CellType.Empty
        );

        CellType cellAtDestination = GridManager.Instance.GetCell(boxNewPos);
        GridManager.Instance.SetCell(
            boxNewPos,
            cellAtDestination == CellType.Target ? CellType.BoxOnTarget : CellType.Box
        );
    }

    private BoxController FindBoxAt(Vector2Int pos)
    {
        BoxController[] boxes = FindObjectsOfType<BoxController>();
        foreach (var box in boxes)
        {
            if (GridManager.Instance.WorldToGrid(box.transform.position) == pos)
                return box;
        }
        return null;
    }
}