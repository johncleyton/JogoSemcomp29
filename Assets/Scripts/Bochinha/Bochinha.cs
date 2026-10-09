using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Lançador do jogador (arrastar e soltar estilo estilingue).
// Tudo que é visual (bola fantasma no ponto de lançamento, pontinhos da mira, ponto previsto de parada)
// é criado por código, então não precisa configurar nada na cena além de launchPoint e cam.
public class BochinhaLauncher : MonoBehaviour
{
    public static BochinhaLauncher Instance;

    [Header("Referências")]
    public Transform launchPoint;
    [Tooltip("Câmera DO MINIGAME. Se vazio usa Camera.main (cuidado: a TelaIntervalo também tem câmera).")]
    public Camera cam;

    [Header("Força do lançamento")]
    public float maxForce = 20f;               // impulso máximo (com massa 1 = velocidade inicial)
    public float distanciaMaxArraste = 3f;     // arraste (unidades do mundo) que dá força máxima
    public float distanciaMinArraste = 0.3f;   // arraste menor que isso cancela o lance (clique acidental)

    [Header("Física das bolas (aplicada em TODA bola lançada, jogador e CPU)")]
    public float arrastoLinear = 2.2f;         // "atrito do chão": maior = para mais rápido e mais perto
    public float arrastoAngular = 4f;
    [Range(0f, 1f)] public float quique = 0.45f;

    [Header("Mira")]
    public float espacoPontos = 0.4f;
    public float tamanhoPonto = 0.16f;
    public int maxPontos = 40;
    public int sortingOrderMira = 50;
    public Color corPoucaForca = new Color(1f, 0.92f, 0.3f);
    public Color corMuitaForca = new Color(1f, 0.25f, 0.2f);

    // --- Estado exposto pro HUD ---
    public bool PodeLancar => podeLancar;
    public bool Arrastando => arrastando;
    /// <summary>0 a 1: força do arraste atual (0 se o arraste ainda é curto demais pra valer).</summary>
    public float PoderNormalizado { get; private set; }
    /// <summary>Disparado quando o jogador solta o dedo/mouse com um arraste curto demais (lance cancelado).</summary>
    public event System.Action ArrasteCancelado;

    private GameObject prefabAtualParaLancar;
    private float massaAtual = 1f;
    private bool podeLancar = false;
    private bool arrastando = false;
    private Vector2 inicioArraste;

    private SpriteRenderer fantasma;    // bola translúcida pulsando no ponto de lançamento
    private SpriteRenderer previsao;    // onde a bola deve parar (sem contar quiques)
    private SpriteRenderer[] pontos;    // pontinhos da mira
    private Vector3 escalaBola = Vector3.one;
    private Sprite spriteCirculo;
    private PhysicsMaterial2D materialBola;

    private static readonly HashSet<string> spritesAvisados = new HashSet<string>();

    void Awake()
    {
        Instance = this;
        if (cam == null) cam = Camera.main;
        if (launchPoint == null) launchPoint = transform;

        materialBola = new PhysicsMaterial2D("BolaBochinha") { bounciness = quique, friction = 0f };
        spriteCirculo = CriarSpriteCirculo();

        fantasma = CriarRenderer("LaunchPreview", sortingOrderMira - 1);
        previsao = CriarRenderer("PrevisaoParada", sortingOrderMira - 1);
        pontos = new SpriteRenderer[maxPontos];
        for (int i = 0; i < maxPontos; i++)
        {
            pontos[i] = CriarRenderer("PontoMira" + i, sortingOrderMira);
            pontos[i].sprite = spriteCirculo;
        }
        EsconderMira();
        fantasma.enabled = false;
    }

    // ----------------------------------------------------------------------------------
    // API usada pelo GameManager
    // ----------------------------------------------------------------------------------

    public void SetupTurn(GameObject prefabParaLancar, bool isBolim)
    {
        prefabAtualParaLancar = prefabParaLancar;

        Rigidbody2D rbPrefab = prefabParaLancar.GetComponent<Rigidbody2D>();
        massaAtual = rbPrefab != null ? Mathf.Max(0.01f, rbPrefab.mass) : 1f;

        // O fantasma copia a aparência da bola que vai ser lançada
        SpriteRenderer srPrefab = prefabParaLancar.GetComponentInChildren<SpriteRenderer>();
        Sprite spriteBola = (srPrefab != null && srPrefab.sprite != null) ? srPrefab.sprite : spriteCirculo;
        Color corBola = srPrefab != null ? srPrefab.color : Color.white;
        escalaBola = srPrefab != null ? srPrefab.transform.lossyScale : prefabParaLancar.transform.lossyScale;

        fantasma.sprite = spriteBola;
        fantasma.color = new Color(corBola.r, corBola.g, corBola.b, 0.65f);
        previsao.sprite = spriteBola;

        fantasma.transform.position = launchPoint.position;
        fantasma.transform.localScale = escalaBola;
        fantasma.enabled = true;

        arrastando = false;
        PoderNormalizado = 0f;
        podeLancar = true;
    }

    /// <summary>Trava o lançamento (fim de rodada / tempo esgotado). Esconde mira e fantasma.</summary>
    public void DesativarLancamento()
    {
        podeLancar = false;
        arrastando = false;
        PoderNormalizado = 0f;
        EsconderMira();
        if (fantasma != null) fantasma.enabled = false;
    }

    // Aplica a MESMA física em qualquer bola (chame também pra bolas lançadas pela CPU)
    public void PrepararBola(Rigidbody2D rb)
    {
        if (rb == null) return;

        rb.gravityScale = 0f;                                   // visão de cima: sem gravidade
        rb.drag = arrastoLinear;                                // freia sozinha, sem deslizar pra sempre
        rb.angularDrag = arrastoAngular;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // não atravessa parede em alta velocidade
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        foreach (Collider2D col in rb.GetComponentsInChildren<Collider2D>())
            col.sharedMaterial = materialBola;

        VerificarSpriteVsCollider(rb);
    }

    // Força necessária pra uma bola de certa massa parar a "distancia" unidades de onde saiu
    // (com arrasto linear: alcance ≈ velocidadeInicial / arrasto)
    public float ForcaParaAlcance(float distancia, float massa)
    {
        return Mathf.Min(distancia * massa * arrastoLinear, maxForce);
    }

    // ----------------------------------------------------------------------------------
    // Diagnóstico: o collider fica no transform (pivot do sprite). Se o PNG tiver uma área
    // transparente grande, ou o pivot não estiver no centro da bolinha, a bola é DESENHADA num lugar
    // e COLIDE em outro. Este aviso aparece no Console (uma vez por sprite) quando isso acontece.
    // ----------------------------------------------------------------------------------
    private void VerificarSpriteVsCollider(Rigidbody2D rb)
    {
        SpriteRenderer sr = rb.GetComponentInChildren<SpriteRenderer>();
        CircleCollider2D cc = rb.GetComponentInChildren<CircleCollider2D>();
        if (sr == null || sr.sprite == null || cc == null) return;

        Vector3 esc = sr.transform.lossyScale;
        Vector2 tam = sr.sprite.bounds.size;
        float ladoSprite = Mathf.Max(tam.x * Mathf.Abs(esc.x), tam.y * Mathf.Abs(esc.y));

        Vector3 escCol = cc.transform.lossyScale;
        float diametroCollider = cc.radius * 2f * Mathf.Max(Mathf.Abs(escCol.x), Mathf.Abs(escCol.y));

        Vector3 centro = Vector3.Scale(sr.sprite.bounds.center, esc);
        float deslocamento = new Vector2(centro.x, centro.y).magnitude;

        bool grande = ladoSprite > diametroCollider * 1.6f;
        bool descentrado = deslocamento > 0.15f;

        if ((grande || descentrado) && spritesAvisados.Add(sr.sprite.name))
        {
            Debug.LogWarning(
                "[Bochinha] O sprite '" + sr.sprite.name + "' (prefab '" + rb.gameObject.name + "') ocupa " +
                ladoSprite.ToString("F1") + " unidades, mas o collider tem " + diametroCollider.ToString("F1") +
                " de diametro (centro do desenho deslocado " + deslocamento.ToString("F2") + " do transform). " +
                "A bola vai colidir longe de onde aparece. Selecione o PNG no Project e use " +
                "Assets > Bochinha > Recortar bola..., depois troque o sprite do prefab pelo '_recorte'.", sr);
        }
    }

    // ----------------------------------------------------------------------------------
    // Input
    // ----------------------------------------------------------------------------------

    void Update()
    {
        if (!podeLancar) return;

        // Fantasma pulsando: mostra claramente DE ONDE a bola sai
        float pulso = 1f + 0.07f * Mathf.Sin(Time.time * 6f);
        fantasma.transform.position = launchPoint.position;
        fantasma.transform.localScale = escalaBola * pulso;

        if (Input.GetMouseButtonDown(0))
        {
            arrastando = true;
            inicioArraste = PosicaoMouseMundo();
        }

        if (!arrastando) return;

        Vector2 arraste = inicioArraste - PosicaoMouseMundo();
        bool valido = arraste.magnitude >= distanciaMinArraste;
        float poder = Mathf.Clamp01(arraste.magnitude / distanciaMaxArraste);
        PoderNormalizado = valido ? poder : 0f;

        if (valido) DesenharMira(arraste.normalized, poder);
        else EsconderMira();

        if (Input.GetMouseButtonUp(0))
        {
            arrastando = false;
            PoderNormalizado = 0f;
            EsconderMira();

            if (valido) Launch(arraste.normalized, poder * maxForce);
            else if (ArrasteCancelado != null) ArrasteCancelado.Invoke();
        }
    }

    private Vector2 PosicaoMouseMundo()
    {
        Vector3 p = cam.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(p.x, p.y);
    }

    private void Launch(Vector2 direcao, float forca)
    {
        podeLancar = false;
        fantasma.enabled = false;

        GameObject novaBola = Instantiate(prefabAtualParaLancar, launchPoint.position, Quaternion.identity);
        Rigidbody2D rb2d = novaBola.GetComponent<Rigidbody2D>();

        if (rb2d != null)
        {
            PrepararBola(rb2d);
            rb2d.AddForce(direcao * forca, ForceMode2D.Impulse);
        }

        BochinhaGameManager.Instance.BolaLancada(novaBola);
    }

    // ----------------------------------------------------------------------------------
    // Visual da mira
    // ----------------------------------------------------------------------------------

    private void DesenharMira(Vector2 direcao, float poder)
    {
        Vector2 origem = launchPoint.position;
        float alcance = (poder * maxForce) / (massaAtual * arrastoLinear); // mesmo cálculo da física real
        int n = Mathf.Min(pontos.Length, Mathf.FloorToInt(alcance / espacoPontos));
        Color cor = Color.Lerp(corPoucaForca, corMuitaForca, poder);

        for (int i = 0; i < pontos.Length; i++)
        {
            if (i < n)
            {
                float t = (float)i / Mathf.Max(1, n);
                pontos[i].enabled = true;
                pontos[i].transform.position = origem + direcao * (espacoPontos * (i + 1));
                pontos[i].transform.localScale = Vector3.one * tamanhoPonto * Mathf.Lerp(1f, 0.55f, t);
                pontos[i].color = cor;
            }
            else
            {
                pontos[i].enabled = false;
            }
        }

        previsao.enabled = true;
        previsao.transform.position = origem + direcao * alcance;
        previsao.transform.localScale = escalaBola;
        previsao.color = new Color(cor.r, cor.g, cor.b, 0.35f);
    }

    private void EsconderMira()
    {
        if (pontos != null)
            foreach (var p in pontos) p.enabled = false;
        if (previsao != null) previsao.enabled = false;
    }

    // ----------------------------------------------------------------------------------
    // Helpers de criação
    // ----------------------------------------------------------------------------------

    private SpriteRenderer CriarRenderer(string nome, int ordem)
    {
        GameObject go = new GameObject(nome);
        // Sem isso o objeto nasce na cena ATIVA (a TelaIntervalo, durante o Awake) e nunca é descarregado
        // junto com o minigame: a cada partida sobravam ~43 objetos soltos.
        SceneManager.MoveGameObjectToScene(go, gameObject.scene);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = ordem;
        return sr;
    }

    // Círculo branco 1x1 unidade gerado por código (não depende de nenhum asset)
    private Sprite CriarSpriteCirculo()
    {
        const int tam = 32;
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float r = tam / 2f;
        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, d <= r - 1f ? 1f : 0f));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f), tam);
    }
}