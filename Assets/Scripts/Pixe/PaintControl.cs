using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class PaintableCanvas : MonoBehaviour
{
    public static event Action<float> paintPercentageUpdated;

    [Header("Brush Settings")]
    [SerializeField] private Color paintColor = Color.red;
    [SerializeField] private int brushRadius = 5;
    [SerializeField] private float winThreshold = 0.95f; // 95% painted to win

    private Texture2D baseTexture;
    private Texture2D paintableTexture;
    private SpriteRenderer spriteRenderer;
    private Camera mainCam;

    private Color[] originalPixels;
    private int totalPaintablePixels = 0;

    void Start()
    {
        mainCam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseTexture = spriteRenderer.sprite.texture;

        originalPixels = baseTexture.GetPixels();

        for (int i = 0; i < originalPixels.Length; i++)
        {
            if (originalPixels[i].a > 0.1f)
            {
                totalPaintablePixels++;
            }
        }

        paintableTexture = new Texture2D(baseTexture.width, baseTexture.height);
        paintableTexture.SetPixels(originalPixels);
        paintableTexture.Apply();

        spriteRenderer.sprite = Sprite.Create(paintableTexture, spriteRenderer.sprite.rect, new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCam.transform.position.z));
            worldPos.z = 0f;

            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                PaintAtWorldPosition(worldPos);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            CheckCompletion();
        }
    }

    private void PaintAtWorldPosition(Vector3 worldPos)
    {
        Bounds bounds = spriteRenderer.bounds;
        float percentX = (worldPos.x - bounds.min.x) / bounds.size.x;
        float percentY = (worldPos.y - bounds.min.y) / bounds.size.y;

        int centerX = Mathf.RoundToInt(percentX * paintableTexture.width);
        int centerY = Mathf.RoundToInt(percentY * paintableTexture.height);

        for (int x = -brushRadius; x <= brushRadius; x++)
        {
            for (int y = -brushRadius; y <= brushRadius; y++)
            {
                if (x * x + y * y <= brushRadius * brushRadius)
                {
                    int pixelX = centerX + x;
                    int pixelY = centerY + y;

                    if (pixelX >= 0 && pixelX < paintableTexture.width && pixelY >= 0 && pixelY < paintableTexture.height)
                    {
                        int pixelIndex = pixelY * paintableTexture.width + pixelX;

                        if (originalPixels[pixelIndex].a > 0.1f)
                        {
                            paintableTexture.SetPixel(pixelX, pixelY, paintColor);
                        }
                    }
                }
            }
        }
        paintableTexture.Apply();
    }

    private void CheckCompletion()
    {
        Color[] currentPixels = paintableTexture.GetPixels();
        int paintedPixelCount = 0;

        for (int i = 0; i < currentPixels.Length; i++)
        {
            if (currentPixels[i] == paintColor)
            {
                paintedPixelCount++;
            }
        }

        // Checa a porcentagem pintada
        float percentagePainted = (float)paintedPixelCount / totalPaintablePixels;
        Debug.Log($"Porcentagem: {percentagePainted * 100f}%");

        paintPercentageUpdated?.Invoke(percentagePainted);

        if (percentagePainted >= winThreshold)
        {
            Debug.Log("Tem que ganhar");
        }
    }
}