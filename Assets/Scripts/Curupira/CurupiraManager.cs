using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Minigame do Curupira
// Cada casa tem uma trilha de pegadas que vem de baixo da tela. As trilhas se cruzam.
// Só a trilha que vai para a casa do Curupira tem as pegadas viradas para trás.
public class CurupiraManager : MinigameBase
{
    public static CurupiraManager Instance;

    [Header("Referencias da Cena")]
    [Tooltip("Se ficar vazio, encontra todas as CasaScript da cena sozinho.")]
    public List<CasaScript> casasCena;
    public CasaScript casaCorreta;

    [Header("Tempo")]
    [Tooltip("É um minigame de pensar: nunca deixa o tempo ficar menor que isso.")]
    public float tempoMinimo = 7f;

    [Header("Sprites das pegadas")]
    [Tooltip("Pé esquerdo (0) e pé direito (1). Alternam ao longo da trilha.")]
    public Sprite[] spritesPegadas;
    public float escalaPegadas = 0.8f;
    public int sortingOrderPegadas = 5;
    [Tooltip("Rotação base do sprite. -90 = dedos do pé apontando para CIMA no PNG.")]
    public float rotacaoBaseSprite = -90f;

    [Header("Trilhas")]
    [Tooltip("Distância aproximada entre uma pegada e a próxima (ao longo da curva).")]
    public float distanciaEntrePegadas = 1f;
    [Tooltip("Quanto cada pé sai para o lado da linha da trilha.")]
    public float afastamentoLateral = 0.35f;
    [Tooltip("A trilha termina essa distância ABAIXO da casa. Maior = mais difícil ler só o final.")]
    public float distanciaDaCasa = 1.6f;
    [Tooltip("Quão curvas são as trilhas. Mais curva = mais cruzamentos.")]
    public float curvatura = 3f;
    [Tooltip("As origens se espalham entre -largura e +largura no eixo X.")]
    public float larguraOrigens = 6f;
    [Tooltip("Altura (Y) onde as trilhas começam.")]
    public float alturaOrigens = -3.2f;
    [Tooltip("Tempo entre uma pegada aparecer e a próxima.")]
    public float intervaloEntrePegadas = 0.15f;

    [Header("Pegadas que somem (opcional)")]
    public bool pegadasSomem = false;
    [Tooltip("Segundos que cada pegada fica visível antes de sumir.")]
    public float vidaDasPegadas = 4f;

    [Header("Feedback")]
    [Tooltip("Tempo que a cena fica parada mostrando o resultado.")]
    public float atrasoResultado = 1.5f;
    [Tooltip("Quando erra, quanto tempo até o fogo acender na casa certa.")]
    public float atrasoRevelarErro = 0.5f;

    class Trilha
    {
        public CasaScript casa;
        public bool ehCurupira;
        public List<SpriteRenderer> pegadas = new List<SpriteRenderer>();
    }

    bool resolvendo;
    float curvaturaAtual;
    float vidaAtual;
    readonly List<Trilha> trilhas = new List<Trilha>();

    void Awake()
    {
        Instance = this;
        curvaturaAtual = curvatura;
        vidaAtual = vidaDasPegadas;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (casasCena == null || casasCena.Count == 0)
            casasCena = new List<CasaScript>(FindObjectsOfType<CasaScript>());

        SortearCasaCurupira();
        GerarTrilhas();
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // Fases avançadas: trilhas mais curvas (mais cruzamentos) e pegadas que somem mais rápido.
        float dif = Mathf.Clamp01(faseAtual / 10f);
        curvaturaAtual = Mathf.Lerp(curvatura * 0.6f, curvatura, dif);
        vidaAtual = Mathf.Lerp(vidaDasPegadas, vidaDasPegadas * 0.6f, dif);

        // O GameManagerRework chama isso depois da cena ativar: refaz as trilhas.
        if (casaCorreta != null) GerarTrilhas();

        return Mathf.Max(tempoGlobalSugerido, tempoMinimo);
    }

    void SortearCasaCurupira()
    {
        if (casasCena == null || casasCena.Count == 0)
        {
            Debug.LogError("Nenhuma casa adicionada na cena");
            return;
        }

        casaCorreta = casasCena[Random.Range(0, casasCena.Count)];
    }

    // ---------- TRILHAS ----------
    void GerarTrilhas()
    {
        LimparTrilhas();
        if (casaCorreta == null || casasCena == null || casasCena.Count == 0) return;
        if (spritesPegadas == null || spritesPegadas.Length == 0) return;

        // Casas da esquerda para a direita
        List<CasaScript> casas = new List<CasaScript>(casasCena);
        casas.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        int n = casas.Count;

        // n origens espalhadas embaixo, da esquerda para a direita
        Vector2[] origens = new Vector2[n];
        float slot = (2f * larguraOrigens) / n;
        for (int i = 0; i < n; i++)
        {
            float x = -larguraOrigens + slot * (i + 0.5f) + Random.Range(-slot * 0.25f, slot * 0.25f);
            float y = alturaOrigens + Random.Range(-0.4f, 0.4f);
            origens[i] = new Vector2(x, y);
        }

        // Sorteia qual origem vai para qual casa: é isso que faz as trilhas se cruzarem
        int[] ordem = SortearOrdemComCruzamentos(n);

        for (int i = 0; i < n; i++)
            CriarTrilha(casas[i], origens[ordem[i]], casas[i] == casaCorreta);

        foreach (Trilha t in trilhas)
            StartCoroutine(RevelarTrilha(t, Random.Range(0f, 0.4f)));
    }

    int[] SortearOrdemComCruzamentos(int n)
    {
        int[] a = new int[n];
        for (int i = 0; i < n; i++) a[i] = i;
        if (n < 3) return a;

        for (int tentativa = 0; tentativa < 20; tentativa++)
        {
            for (int i = n - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = a[i]; a[i] = a[j]; a[j] = tmp;
            }
            if (ContarInversoes(a) >= 2) break; // pelo menos 2 pares de trilhas se cruzam
        }
        return a;
    }

    int ContarInversoes(int[] a)
    {
        int c = 0;
        for (int i = 0; i < a.Length; i++)
            for (int j = i + 1; j < a.Length; j++)
                if (a[i] > a[j]) c++;
        return c;
    }

    void CriarTrilha(CasaScript casa, Vector2 origem, bool ehCurupira)
    {
        Vector2 fim = (Vector2)casa.transform.position + Vector2.down * distanciaDaCasa;
        Vector2 dirGeral = (fim - origem).normalized;
        Vector2 perp = new Vector2(-dirGeral.y, dirGeral.x);
        float desvio = Random.Range(0.4f, 1f) * curvaturaAtual * (Random.value < 0.5f ? -1f : 1f);
        Vector2 controle = (origem + fim) * 0.5f + perp * desvio;

        // Amostra a curva para poder andar nela em passos iguais
        const int amostras = 40;
        Vector2[] pts = new Vector2[amostras + 1];
        float[] acum = new float[amostras + 1];
        for (int i = 0; i <= amostras; i++)
        {
            pts[i] = Bezier(origem, controle, fim, i / (float)amostras);
            if (i > 0) acum[i] = acum[i - 1] + Vector2.Distance(pts[i - 1], pts[i]);
        }
        float comprimento = acum[amostras];

        int quantidade = Mathf.Clamp(Mathf.FloorToInt(comprimento / distanciaEntrePegadas) + 1, 3, 20);
        float passo = comprimento / (quantidade - 1);

        Trilha trilha = new Trilha { casa = casa, ehCurupira = ehCurupira };

        for (int i = 0; i < quantidade; i++)
        {
            Vector2 pos, tang;
            PosicaoNoCaminho(pts, acum, i * passo, out pos, out tang);

            // pé esquerdo / direito alternando dos dois lados da linha
            Vector2 lateral = new Vector2(-tang.y, tang.x);
            pos += lateral * (i % 2 == 0 ? afastamentoLateral : -afastamentoLateral);

            // Gente normal: dedos para a frente. Curupira: dedos para TRÁS.
            Vector2 dedos = ehCurupira ? -tang : tang;
            float angulo = Mathf.Atan2(dedos.y, dedos.x) * Mathf.Rad2Deg + rotacaoBaseSprite;

            GameObject go = new GameObject("Pegada_" + casa.name + "_" + i);
            go.transform.SetParent(transform, true);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angulo);
            go.transform.localScale = Vector3.one * escalaPegadas;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spritesPegadas[i % spritesPegadas.Length];
            sr.sortingOrder = sortingOrderPegadas;
            sr.color = new Color(1f, 1f, 1f, 0f);

            trilha.pegadas.Add(sr);
        }

        trilhas.Add(trilha);
    }

    static Vector2 Bezier(Vector2 a, Vector2 c, Vector2 b, float t)
    {
        float u = 1f - t;
        return u * u * a + 2f * u * t * c + t * t * b;
    }

    static void PosicaoNoCaminho(Vector2[] pts, float[] acum, float s, out Vector2 pos, out Vector2 tang)
    {
        int j = 0;
        while (j < pts.Length - 2 && acum[j + 1] < s) j++;
        float seg = acum[j + 1] - acum[j];
        float k = seg > 0.0001f ? (s - acum[j]) / seg : 0f;
        pos = Vector2.Lerp(pts[j], pts[j + 1], k);
        tang = (pts[j + 1] - pts[j]).normalized;
    }

    void LimparTrilhas()
    {
        // As coroutines de revelar/sumir terminam sozinhas quando o SpriteRenderer é destruído.
        foreach (Trilha t in trilhas)
            foreach (SpriteRenderer sr in t.pegadas)
                if (sr != null) Destroy(sr.gameObject);
        trilhas.Clear();
    }

    IEnumerator RevelarTrilha(Trilha t, float atraso)
    {
        yield return new WaitForSeconds(atraso);
        foreach (SpriteRenderer sr in t.pegadas)
        {
            if (sr == null) yield break;
            StartCoroutine(VidaDaPegada(sr));
            yield return new WaitForSeconds(intervaloEntrePegadas);
        }
    }

    IEnumerator VidaDaPegada(SpriteRenderer sr)
    {
        yield return Fade(sr, 0f, 1f, 0.15f);
        if (!pegadasSomem || resolvendo) yield break;

        yield return new WaitForSeconds(vidaAtual);
        if (resolvendo) yield break;
        yield return Fade(sr, 1f, 0f, 0.5f);
    }

    IEnumerator Fade(SpriteRenderer sr, float de, float para, float duracao)
    {
        float t = 0f;
        while (t < duracao)
        {
            if (sr == null) yield break;
            t += Time.deltaTime;
            sr.color = new Color(1f, 1f, 1f, Mathf.Lerp(de, para, t / duracao));
            yield return null;
        }
        if (sr != null) sr.color = new Color(1f, 1f, 1f, para);
    }

    // Mostra a resposta: acende o fogo na casa do Curupira e destaca a trilha dele
    void RevelarResposta()
    {
        if (casaCorreta != null) casaCorreta.MostrarFogo();

        foreach (Trilha t in trilhas)
            foreach (SpriteRenderer sr in t.pegadas)
                if (sr != null)
                    StartCoroutine(Fade(sr, sr.color.a, t.ehCurupira ? 1f : 0.25f, 0.3f));
    }

    // ---------- RESULTADO ----------
    public void VerificarCasa(CasaScript casaClicada)
    {
        if (jogoFinalizado || resolvendo) return;
        resolvendo = true;

        if (casaClicada == casaCorreta)
        {
            Debug.Log("Encontrou a casa do curupira! Vitória!");
            RevelarResposta();
            VencerComAtraso(atrasoResultado);
        }
        else
        {
            Debug.Log("Casa errada clicada! Derrota!");
            casaClicada.Tremer();
            StartCoroutine(RevelarDepoisDoErro());
            PerderComAtraso(atrasoResultado);
        }
    }

    IEnumerator RevelarDepoisDoErro()
    {
        yield return new WaitForSecondsRealtime(atrasoRevelarErro);
        RevelarResposta();
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado || resolvendo) return;
        resolvendo = true;

        Debug.Log("Tempo esgotado! O curupira fugiu...");
        RevelarResposta();
        base.TempoEsgotado(); // Perder() já espera 2s por padrão
    }
}