using UnityEngine;

public class CameraPanMobile : MonoBehaviour
{
    [Header("Limite do Mapa (opcional)")]
    [Tooltip("Arraste aqui um BoxCollider2D que demarque a área jogável do cenário, para impedir que a câmera saia da arte do mapa. Deixe vazio para não limitar.")]
    public BoxCollider2D limitesDoMapa;

    private Vector3 dragOrigin;
    private Camera cam;
    private float meiaAltura;
    private float meiaLargura;

    void Awake()
    {
        // CORRIGIDO: Camera.main estava sendo chamado duas vezes por frame
        // dentro do Update (busca por tag, custosa em mobile). Agora é
        // buscado e guardado uma única vez.
        cam = Camera.main;
    }

    void Start()
    {
        if (limitesDoMapa != null && cam != null)
        {
            meiaAltura = cam.orthographicSize;
            meiaLargura = meiaAltura * cam.aspect;
        }
    }

    void Update()
    {
        if (cam == null) return;

        // CORRIGIDO: para de mover a câmera quando o jogo já terminou
        // (vitória ou derrota), em vez de continuar reagindo ao arrastar.
        if (GameManagerCapivara.Instance != null && GameManagerCapivara.Instance.JogoEncerrado) return;

        // Funciona tanto para mouse (teste no editor) quanto para touch único no celular
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 novaPosicao = cam.transform.position + difference;

            // CORRIGIDO: sem isso, era possível arrastar a câmera infinitamente
            // para fora do cenário desenhado, revelando área vazia.
            if (limitesDoMapa != null)
            {
                novaPosicao = LimitarDentroDoMapa(novaPosicao);
            }

            cam.transform.position = novaPosicao;
        }
    }

    private Vector3 LimitarDentroDoMapa(Vector3 posicao)
    {
        Bounds limites = limitesDoMapa.bounds;

        float minX = limites.min.x + meiaLargura;
        float maxX = limites.max.x - meiaLargura;
        float minY = limites.min.y + meiaAltura;
        float maxY = limites.max.y - meiaAltura;

        // Se o mapa for menor que a tela numa direção, centraliza em vez de travar
        posicao.x = (minX < maxX) ? Mathf.Clamp(posicao.x, minX, maxX) : limites.center.x;
        posicao.y = (minY < maxY) ? Mathf.Clamp(posicao.y, minY, maxY) : limites.center.y;

        return posicao;
    }
}
