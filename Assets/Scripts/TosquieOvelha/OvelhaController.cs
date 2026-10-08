using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum EstadoOvelha { MuitoNova, EmPerfeitoEstado, MuitoVelha }

public class OvelhaController : MonoBehaviour
{
    [Header("Estado")]
    public EstadoOvelha estadoAtual;

    [Header("Visual - preencha o campo correspondente ao tipo do seu objeto")]
    [Tooltip("Caso comum: a Ovelha é um prefab de mundo com Sprite Renderer no root.")]
    public SpriteRenderer spriteRenderer;
    [Tooltip("Só use se a ovelha for uma Image de UI dentro de um Canvas.")]
    public Image imagemUI;

    [Header("Sprites por estado (arraste no PREFAB, não na instância da cena)")]
    public Sprite spriteMuitoNova;
    public Sprite spriteEmPerfeitoEstado;
    public Sprite spriteMuitoVelha;

    [Tooltip("Sprite sem lã (adulta-e-velha-sem-la). Aparece quando a ovelha termina de ser tosquiada.")]
    public Sprite spriteTosquiada;

    [Header("Área da lã (as faixas com tag 'La' se distribuem sozinhas dentro dela)")]
    [Tooltip("Centro do trecho com lã, em unidades, relativo ao pivot da ovelha. Selecione a ovelha pra ver o retângulo ciano.")]
    public Vector2 centroAreaLa = new Vector2(0f, 1.2f);
    [Tooltip("Largura e altura do trecho com lã, em unidades.")]
    public Vector2 tamanhoAreaLa = new Vector2(2.4f, 0.9f);
    [Tooltip("Quantas faixas (pedaços de lã) a ovelha tem. As que faltarem são criadas automaticamente.")]
    public int quantidadeFaixas = 5;
    [Tooltip("Só afeta faixas que já tenham SpriteRenderer. As criadas por código são sempre invisíveis (use o retângulo ciano pra ajustar).")]
    public bool mostrarFaixas = false;

    // Limites da lã em coordenadas de MUNDO (usados pelo TosquieManager pra ajustar a tosquiadora)
    public float EsquerdaDaLaX => transform.position.x + centroAreaLa.x - tamanhoAreaLa.x * 0.5f;
    public float DireitaDaLaX => transform.position.x + centroAreaLa.x + tamanhoAreaLa.x * 0.5f;
    public float BaseDaLaY => transform.position.y + centroAreaLa.y - tamanhoAreaLa.y * 0.5f;

    private Vector3 escalaOriginal;
    private Vector3 posRepouso;
    private Coroutine feedbackAtual;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        escalaOriginal = transform.localScale;
        posRepouso = transform.position;

        AplicarAreaLa();
        if (!mostrarFaixas) EsconderFaixas();
    }

    /// <summary>
    /// Distribui as faixas filhas com tag 'La' lado a lado dentro da área da lã.
    /// Funciona também no editor: botão direito no componente > "Aplicar área da lã às faixas".
    /// Pressupõe faixas FILHAS DIRETAS da ovelha, com BoxCollider2D de tamanho 1x1 e offset 0.
    /// </summary>
    [ContextMenu("Aplicar área da lã às faixas")]
    public void AplicarAreaLa()
    {
        System.Collections.Generic.List<Transform> faixas = new System.Collections.Generic.List<Transform>();
        foreach (Transform filho in transform)
            if (filho.CompareTag("La")) faixas.Add(filho);

        // Cria as faixas que faltam: filho com tag 'La' + BoxCollider2D trigger 1x1 (sem sprite)
        for (int i = faixas.Count; i < quantidadeFaixas; i++)
        {
            GameObject go = new GameObject("FaixaLa" + (i + 1));
            go.transform.SetParent(transform, false);
            go.tag = "La";
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;
            faixas.Add(go.transform);
        }

        if (faixas.Count == 0) return;

        Vector3 esc = transform.lossyScale;
        float larg = tamanhoAreaLa.x / faixas.Count;

        for (int i = 0; i < faixas.Count; i++)
        {
            float x = centroAreaLa.x - tamanhoAreaLa.x * 0.5f + larg * (i + 0.5f);
            faixas[i].localPosition = new Vector3(x / esc.x, centroAreaLa.y / esc.y, 0f);
            faixas[i].localScale = new Vector3(larg / esc.x, tamanhoAreaLa.y / esc.y, 1f);
        }
    }

    void EsconderFaixas()
    {
        foreach (Transform filho in transform)
        {
            if (!filho.CompareTag("La")) continue;
            SpriteRenderer sr = filho.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + new Vector3(centroAreaLa.x, centroAreaLa.y, 0f),
                            new Vector3(tamanhoAreaLa.x, tamanhoAreaLa.y, 0.01f));
    }

    public void Inicializar(EstadoOvelha estado)
    {
        estadoAtual = estado;
        AtualizarVisual();
    }

    void AtualizarVisual()
    {
        Sprite spriteCorreto = ObterSpriteDoEstado(estadoAtual);
        if (spriteCorreto == null)
        {
            Debug.LogWarning("OvelhaController: nenhum sprite atribuído para o estado " + estadoAtual +
                             ". Abra o PREFAB da Ovelha e arraste os 3 sprites nos campos.", this);
            return;
        }

        if (spriteRenderer != null) spriteRenderer.sprite = spriteCorreto;
        if (imagemUI != null) imagemUI.sprite = spriteCorreto;
    }

    Sprite ObterSpriteDoEstado(EstadoOvelha estado)
    {
        switch (estado)
        {
            case EstadoOvelha.MuitoNova: return spriteMuitoNova;
            case EstadoOvelha.EmPerfeitoEstado: return spriteEmPerfeitoEstado;
            case EstadoOvelha.MuitoVelha: return spriteMuitoVelha;
            default: return null;
        }
    }

    /// <summary>Troca pro sprite sem lã (chamado quando a ovelha é totalmente tosquiada).</summary>
    public void MostrarTosquiada()
    {
        if (spriteTosquiada == null) return;
        if (spriteRenderer != null) spriteRenderer.sprite = spriteTosquiada;
        if (imagemUI != null) imagemUI.sprite = spriteTosquiada;
    }

    // ------------------------------------------------------------------
    // Transições (tudo por código, não depende de Animator)
    // ------------------------------------------------------------------

    /// <summary>Desliza da posição atual até 'destino'. Use com: yield return ovelha.Entrar(...)</summary>
    public Coroutine Entrar(Vector3 destino, float duracao)
    {
        return StartCoroutine(EntrarRoutine(destino, duracao));
    }

    IEnumerator EntrarRoutine(Vector3 destino, float duracao)
    {
        yield return Deslizar(transform.position, destino, duracao);
        posRepouso = destino;
    }

    /// <summary>Desliza pra fora (deslocamento relativo) e se destrói no fim.</summary>
    public void SairDaTela(Vector3 deslocamento, float duracao)
    {
        StopAllCoroutines();
        transform.localScale = escalaOriginal;
        StartCoroutine(SairRoutine(deslocamento, duracao));
    }

    IEnumerator SairRoutine(Vector3 deslocamento, float duracao)
    {
        Vector3 inicio = transform.position;
        yield return Deslizar(inicio, inicio + deslocamento, duracao);
        Destroy(gameObject);
    }

    IEnumerator Deslizar(Vector3 de, Vector3 para, float duracao)
    {
        float t = 0f;
        while (t < duracao)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(de, para, Mathf.SmoothStep(0f, 1f, t / duracao));
            yield return null;
        }
        transform.position = para;
    }

    // ------------------------------------------------------------------
    // Feedback
    // ------------------------------------------------------------------

    /// <summary>Tremidinha horizontal (pente errado).</summary>
    public void Balancar()
    {
        IniciarFeedback(BalancarRoutine());
    }

    /// <summary>Squash rápido (pedaço de lã cortado).</summary>
    public void Pulinho()
    {
        IniciarFeedback(PulinhoRoutine());
    }

    void IniciarFeedback(IEnumerator rotina)
    {
        if (feedbackAtual != null) StopCoroutine(feedbackAtual);
        transform.position = posRepouso;
        transform.localScale = escalaOriginal;
        feedbackAtual = StartCoroutine(rotina);
    }

    IEnumerator BalancarRoutine()
    {
        const float dur = 0.3f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float amortecimento = 1f - t / dur;
            float x = Mathf.Sin(t * 60f) * 0.12f * amortecimento;
            transform.position = posRepouso + new Vector3(x, 0f, 0f);
            yield return null;
        }
        transform.position = posRepouso;
    }

    IEnumerator PulinhoRoutine()
    {
        const float dur = 0.18f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin((t / dur) * Mathf.PI); // 0 -> 1 -> 0
            transform.localScale = new Vector3(
                escalaOriginal.x * (1f + 0.12f * k),
                escalaOriginal.y * (1f - 0.10f * k),
                escalaOriginal.z);
            yield return null;
        }
        transform.localScale = escalaOriginal;
    }
}