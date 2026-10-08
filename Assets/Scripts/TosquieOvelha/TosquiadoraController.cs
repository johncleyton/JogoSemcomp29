using System.Collections;
using UnityEngine;

// Requisito de física (igual ao que você já tem): a Tosquiadora precisa de Collider2D (Is Trigger)
// + Rigidbody2D (Kinematic), e cada pedaço de lã precisa de Collider2D com a tag "La".
public class TosquiadoraController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float velocidadeHorizontal = 6f;
    public float velocidadeQueda = 20f;
    [Tooltip("Velocidade com que ela volta pro topo depois de cortar ou errar.")]
    public float velocidadeRetorno = 25f;
    public float limiteEsquerdo = -2.5f;
    public float limiteDireito = 2.5f;
    public float alturaInicial = 3f;
    public float limiteErroY = -4f;

    // Ajustado pelo TosquieManager conforme a fase (1 = normal)
    [HideInInspector] public float multiplicadorVelocidade = 1f;

    // Controlado pelo TosquieManager: só anda/cai durante a fase de tosquia
    [HideInInspector] public bool PodeAgir = false;

    [Header("Visual da ferramenta")]
    [Tooltip("SpriteRenderer da tosquiadora. Vazio = pega do próprio objeto ou de um filho.")]
    public SpriteRenderer visual;
    [Tooltip("tosador-afiado: usado na ovelha EmPerfeitoEstado (pente menor).")]
    public Sprite spriteAfiado;
    [Tooltip("tosador-para-velha: usado na ovelha MuitoVelha (pente maior).")]
    public Sprite spriteParaVelha;

    [Header("Dificuldade / mira")]
    [Tooltip("Largura da área de corte (collider da lâmina). Menor = mais difícil. Cada faixa de lã tem ~0.6 de largura.")]
    public float larguraLamina = 0.6f;
    [Tooltip("Transparência da tosquiadora enquanto o jogador ainda não escolheu o pente (ela só se move depois).")]
    [Range(0f, 1f)] public float alphaEmEspera = 0.5f;

    private enum Estado { Movendo, Caindo, Subindo }
    private Estado estado = Estado.Movendo;
    private int direcao = 1;

    void Awake()
    {
        if (visual == null) visual = GetComponentInChildren<SpriteRenderer>();
        AjustarLamina();
    }

    // O collider original era largo: qualquer queda dentro da varredura acertava alguma faixa (15 de 16 no teste).
    void AjustarLamina()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) col = GetComponentInChildren<BoxCollider2D>();
        if (col == null)
        {
            Debug.LogWarning("TosquiadoraController: não achei BoxCollider2D, então 'Largura Lamina' não foi aplicada.", this);
            return;
        }

        float esc = Mathf.Max(0.0001f, Mathf.Abs(col.transform.lossyScale.x));
        col.size = new Vector2(larguraLamina / esc, col.size.y);
        col.offset = new Vector2(0f, col.offset.y);
    }

    /// <summary>Mostra a ferramenta certa pro tipo de ovelha que o jogador escolheu.</summary>
    public void DefinirFerramenta(EstadoOvelha estadoDaOvelha)
    {
        if (visual == null) return;
        Sprite s = estadoDaOvelha == EstadoOvelha.MuitoVelha ? spriteParaVelha : spriteAfiado;
        if (s != null) visual.sprite = s;
    }

    // Apagada = "ainda não é a sua vez"; opaca = ativa (jogador escolheu o pente e pode clicar)
    void AtualizarTransparencia()
    {
        if (visual == null) return;
        bool ativa = PodeAgir || estado != Estado.Movendo;
        Color c = visual.color;
        c.a = ativa ? 1f : alphaEmEspera;
        visual.color = c;
    }

    void Update()
    {
        AtualizarTransparencia();
        TosquieManager gm = TosquieManager.Instance;

        // Voltar pro topo continua mesmo depois que o manager desliga PodeAgir ou encerra o jogo
        // (Vencer/Perder têm 2s de delay; sem isso ela ficava congelada no meio da tela).
        if (estado == Estado.Subindo)
        {
            Subir();
            return;
        }

        if (gm != null && gm.JogoEncerrado) return;
        if (!PodeAgir) return;

        if (estado == Estado.Movendo) Mover();
        else Cair(gm);
    }

    void Mover()
    {
        Vector3 p = transform.position;
        p.x += direcao * velocidadeHorizontal * multiplicadorVelocidade * Time.deltaTime;

        if (p.x >= limiteDireito) { p.x = limiteDireito; direcao = -1; }
        else if (p.x <= limiteEsquerdo) { p.x = limiteEsquerdo; direcao = 1; }

        transform.position = p;

        if (Input.GetMouseButtonDown(0)) estado = Estado.Caindo;
    }

    void Cair(TosquieManager gm)
    {
        Vector3 p = transform.position;
        p.y -= velocidadeQueda * Time.deltaTime;

        if (p.y <= limiteErroY)
        {
            p.y = limiteErroY;
            transform.position = p;
            estado = Estado.Subindo;
            if (gm != null) gm.TentativaFalhou();
            return;
        }

        transform.position = p;
    }

    void Subir()
    {
        Vector3 p = transform.position;
        p.y += velocidadeRetorno * Time.deltaTime;

        if (p.y >= alturaInicial)
        {
            p.y = alturaInicial;
            estado = Estado.Movendo;
        }

        transform.position = p;
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        // Só corta caindo (não corta enquanto sobe ou anda)
        if (!PodeAgir || estado != Estado.Caindo) return;
        if (!outro.CompareTag("La")) return;

        outro.enabled = false; // não conta duas vezes
        StartCoroutine(AnimarLaCortada(outro.transform));

        estado = Estado.Subindo;
        if (TosquieManager.Instance != null) TosquieManager.Instance.CortouLa();
    }

    // O pedaço de lã sobe e encolhe em vez de sumir de uma vez
    IEnumerator AnimarLaCortada(Transform la)
    {
        if (la == null) yield break;

        Vector3 posIni = la.localPosition;
        Vector3 escIni = la.localScale;
        const float dur = 0.25f;
        float t = 0f;

        while (t < dur)
        {
            if (la == null) yield break; // a ovelha pode ter sido destruída
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            la.localPosition = posIni + Vector3.up * (0.8f * k);
            la.localScale = escIni * (1f - k);
            yield return null;
        }

        if (la != null) Destroy(la.gameObject);
    }

    // Chamado pelo TosquieManager no começo de cada rodada de tosquia
    public void ResetarParaTopo()
    {
        float meio = (limiteEsquerdo + limiteDireito) * 0.5f;
        transform.position = new Vector3(meio, alturaInicial, transform.position.z);
        estado = Estado.Movendo;
        direcao = 1;
    }
}