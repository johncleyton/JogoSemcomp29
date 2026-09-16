using UnityEngine;

public class ParallaxLoopUI : MonoBehaviour
{
    [Header("Fundo (cobre a tela inteira, várias cópias em loop)")]
    public Transform[] backgroundPieces;
    public float backgroundScrollSpeed = 2f;
    public bool ajustarEscalaDoFundo = true;

    [Header("Onda (peça única, estática, escala proporcional pra caber na largura)")]
    public Transform wavePiece;
    public bool ajustarEscalaDaOnda = true;

    private float bgImageWidth;
    private float bgStartX;

    void Start()
    {
        if (backgroundPieces.Length > 0)
        {
            if (ajustarEscalaDoFundo)
            {
                foreach (Transform piece in backgroundPieces)
                    AjustarAlturaTela(piece);
            }

            SpriteRenderer sr = backgroundPieces[0].GetComponent<SpriteRenderer>();
            bgImageWidth = sr.bounds.size.x;
            bgStartX = backgroundPieces[0].position.x;

            for (int i = 0; i < backgroundPieces.Length; i++)
            {
                Vector3 pos = backgroundPieces[i].position;
                pos.x = bgStartX + (i * bgImageWidth);
                pos.y = backgroundPieces[0].position.y;
                backgroundPieces[i].position = pos;
            }
        }

        if (wavePiece != null && ajustarEscalaDaOnda)
        {
            AjustarLarguraTela(wavePiece);
        }
    }

    // Escala uniforme pra cobrir a ALTURA da câmera (usado pro fundo)
    private void AjustarAlturaTela(Transform piece)
    {
        SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
        Camera cam = Camera.main;

        if (sr == null || cam == null || !cam.orthographic) return;

        float alturaCamera = cam.orthographicSize * 2f;
        float alturaSprite = sr.sprite.bounds.size.y;

        float escalaNecessaria = alturaCamera / alturaSprite;

        Vector3 escala = piece.localScale;
        escala.x = escalaNecessaria;
        escala.y = escalaNecessaria;
        piece.localScale = escala;
    }

    // Escala uniforme pra cobrir a LARGURA da câmera (usado pra onda)
    private void AjustarLarguraTela(Transform piece)
    {
        SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
        Camera cam = Camera.main;

        if (sr == null || cam == null || !cam.orthographic) return;

        float alturaCamera = cam.orthographicSize * 2f;
        float larguraCamera = alturaCamera * cam.aspect;

        float larguraSprite = sr.sprite.bounds.size.x;

        float escalaNecessaria = larguraCamera / larguraSprite;

        Vector3 escala = piece.localScale;
        escala.x = escalaNecessaria;
        escala.y = escalaNecessaria;
        piece.localScale = escala;
    }

    void Update()
    {
        if (backgroundPieces.Length == 0) return;

        foreach (Transform piece in backgroundPieces)
        {
            Vector3 pos = piece.position;
            pos.x -= backgroundScrollSpeed * Time.deltaTime;
            piece.position = pos;
        }

        foreach (Transform piece in backgroundPieces)
        {
            if (piece.position.x <= bgStartX - bgImageWidth)
            {
                Vector3 pos = piece.position;
                pos.x += bgImageWidth * backgroundPieces.Length;
                piece.position = pos;
            }
        }
    }
}