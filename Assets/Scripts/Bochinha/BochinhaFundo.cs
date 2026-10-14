using UnityEngine;

// Coloque no objeto "background" (o que tem o SpriteRenderer do chão).
// Escala o sprite de forma UNIFORME (sem esticar) pra cobrir a tela inteira e centraliza na câmera.
// Se a arte não for 16:9, um pouco da borda é cortado (em vez de esticar a imagem).
[RequireComponent(typeof(SpriteRenderer))]
public class BochinhaFundo : MonoBehaviour
{
    public Camera cam; // câmera do minigame (vazio = Camera.main)

    void Start()
    {
        Ajustar();
    }

    [ContextMenu("Ajustar agora")]
    public void Ajustar()
    {
        if (cam == null) cam = Camera.main;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (cam == null || sr.sprite == null) return;

        float alturaCam = cam.orthographicSize * 2f;
        float larguraCam = alturaCam * (16f / 9f); // resolução forçada do jogo

        Vector2 tamanhoSprite = sr.sprite.bounds.size; // tamanho sem escala
        float escala = Mathf.Max(larguraCam / tamanhoSprite.x, alturaCam / tamanhoSprite.y);

        // Compensa a escala do pai (o "Background" pode ter escala diferente de 1)
        Vector3 pai = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(escala / pai.x, escala / pai.y, 1f);

        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);
    }
}