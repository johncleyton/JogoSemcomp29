using System.Collections;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    public float moveSpeed = 8f;

    public void MoveTo(Vector2Int newGridPos)
    {
        Vector3 worldPos = GridManager.Instance.GridToWorld(newGridPos);
        StartCoroutine(MoveRoutine(worldPos));
    }

    private IEnumerator MoveRoutine(Vector3 targetWorldPos)
    {
        while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetWorldPos;
    }
}