using UnityEngine;

public class ParallaxLoopUI : MonoBehaviour
{
    [Tooltip("Velocidade do scroll em pixels por segundo")]
    public float scrollSpeed = 50f;

    [Tooltip("As duas (ou mais) copias da mesma imagem, lado a lado")]
    public RectTransform[] backgroundPieces;

    private float imageWidth;
    private float startX;

    void Start()
    {
        if (backgroundPieces.Length == 0) return;

        imageWidth = backgroundPieces[0].rect.width;

        startX = backgroundPieces[0].anchoredPosition.x;

        for (int i = 0; i < backgroundPieces.Length; i++)
        {
            Vector2 pos = backgroundPieces[i].anchoredPosition;
            pos.x = startX + (i * imageWidth);
            pos.y = backgroundPieces[0].anchoredPosition.y;
            backgroundPieces[i].anchoredPosition = pos;
        }
    }

    void Update()
    {
        foreach (RectTransform piece in backgroundPieces)
        {
            Vector2 pos = piece.anchoredPosition;
            pos.x -= scrollSpeed * Time.deltaTime;
            piece.anchoredPosition = pos;
        }
        foreach (RectTransform piece in backgroundPieces)
        {
            if (piece.anchoredPosition.x <= startX - imageWidth)
            {
                Vector2 pos = piece.anchoredPosition;
                pos.x += imageWidth * backgroundPieces.Length;
                piece.anchoredPosition = pos;
            }
        }
    }
}