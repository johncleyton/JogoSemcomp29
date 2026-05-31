using System.Collections;
using UnityEngine;

public class UIShake : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 originalPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalPos = rectTransform.anchoredPosition;
    }

    [ContextMenu("Testar Tremor na UI")]
    public void TestarShake()
    {
        StopAllCoroutines();
        StartCoroutine(Shake(0.2f, 20f));
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            rectTransform.anchoredPosition = new Vector2(originalPos.x + x, originalPos.y + y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = originalPos;
    }
}