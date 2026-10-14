using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TurnoHUD { Bolim, Jogador, Adversario, Aguardando, Fim }

/// <summary>
/// HUD do minigame da Bochinha, 100% criado por código (não precisa montar Canvas nem arrastar nada).
/// O BochinhaGameManager adiciona este componente sozinho se ele não existir na cena.
/// Mostra: intro, turno, bochas restantes de cada time, dica de como lançar, barra de força durante o arraste,
/// alerta de tempo acabando e o resultado. O destaque da bola vencedora continua no BochinhaScoreManager.
/// Obs: os textos estão sem acento de propósito (a fonte padrão do TMP pode não ter os glifos).
/// </summary>
public class BochinhaHUD : MonoBehaviour
{
    [Header("Textos")]
    public string tituloIntro = "BOCHINHA!";
    public string subtituloIntro = "A bola mais perto do bolim vence!";
    public string dicaBolim = "PUXE PARA TRAS E SOLTE PARA LANCAR O BOLIM!";
    public string dicaBocha = "PUXE PARA TRAS E SOLTE!";
    public string dicaArrasteCurto = "PUXE MAIS PARA LANCAR!";
    public string avisoTempo = "TEMPO ACABANDO!";

    [Header("Cores")]
    public Color corJogador = new Color(1f, 0.82f, 0.25f);   // bola amarela
    public Color corAdversario = new Color(0.2f, 0.8f, 1f);  // bola azul
    public Color corVitoria = new Color(0.4f, 1f, 0.45f);
    public Color corDerrota = new Color(1f, 0.3f, 0.25f);

    [Header("Ajustes")]
    public int ordemCanvas = 20;          // aumente se o CanvasHUD da TelaIntervalo ficar por cima
    public float duracaoIntro = 1.6f;
    [Range(0f, 1f)] public float tempoAcabandoEm = 0.7f; // a partir de quanto do tempo o alerta aparece
    public bool esconderTextosAntigos = true;            // desliga o turnText/scoreText antigos do GameManager

    private BochinhaGameManager gm;
    private Canvas canvas;
    private Sprite spriteCirculo;

    private Image vinheta, forcaFill;
    private TextMeshProUGUI txtTurno, txtDica, txtTempo, txtResultado, txtIntro, txtSub, txtForca;
    private RectTransform rtTurno, rtDica, rtIntro, rtResultado;
    private CanvasGroup grupoIntro;
    private GameObject grupoForca;
    private readonly List<Image> pontosJogador = new List<Image>();
    private readonly List<Image> pontosAdversario = new List<Image>();
    private Coroutine popTurno;
    private bool finalizado, derrotaPorTempo;
    private float dicaCurtaTimer;

    // ------------------------------------------------------------------ Ciclo de vida

    void Awake()
    {
        // Construído no Awake pra o GameManager poder chamar a API logo no Start dele
        spriteCirculo = CriarSpriteCirculo();
        ConstruirUI();
    }

    void Start()
    {
        gm = BochinhaGameManager.Instance;

        if (esconderTextosAntigos && gm != null)
        {
            if (gm.turnText != null) gm.turnText.gameObject.SetActive(false);
            if (gm.scoreText != null) gm.scoreText.gameObject.SetActive(false);
        }

        if (BochinhaLauncher.Instance != null) BochinhaLauncher.Instance.ArrasteCancelado += OnArrasteCancelado;
        StartCoroutine(Intro());
    }

    void OnDestroy()
    {
        if (BochinhaLauncher.Instance != null) BochinhaLauncher.Instance.ArrasteCancelado -= OnArrasteCancelado;
    }

    private void OnArrasteCancelado()
    {
        dicaCurtaTimer = 1.3f;
    }

    // ------------------------------------------------------------------ API pública

    public void MostrarTurno(TurnoHUD turno)
    {
        string texto;
        Color cor;
        switch (turno)
        {
            case TurnoHUD.Bolim: texto = "LANCE O BOLIM!"; cor = Color.white; break;
            case TurnoHUD.Jogador: texto = "SUA VEZ!"; cor = corJogador; break;
            case TurnoHUD.Adversario: texto = "VEZ DO ADVERSARIO..."; cor = corAdversario; break;
            case TurnoHUD.Aguardando: texto = "AGUARDE..."; cor = new Color(0.85f, 0.85f, 0.85f); break;
            default: texto = "FIM DA RODADA!"; cor = Color.white; break;
        }

        txtTurno.text = texto;
        txtTurno.color = cor;

        if (popTurno != null) StopCoroutine(popTurno);
        popTurno = StartCoroutine(PopTurno());
    }

    public void AtualizarContagem(int restantesJogador, int restantesAdversario, int total)
    {
        GarantirPontos(pontosJogador, total, new Vector2(0f, 0f), 1, "PontoJogador");
        GarantirPontos(pontosAdversario, total, new Vector2(1f, 0f), -1, "PontoAdversario");
        PintarPontos(pontosJogador, restantesJogador, corJogador);
        PintarPontos(pontosAdversario, restantesAdversario, corAdversario);
    }

    public void MostrarResultado(string texto, Color cor, bool porTempo)
    {
        if (finalizado) return;
        finalizado = true;
        derrotaPorTempo = porTempo;

        txtResultado.text = texto;
        txtResultado.color = cor;
        txtResultado.gameObject.SetActive(true);
        if (rtIntro != null) rtIntro.gameObject.SetActive(false);
        StartCoroutine(PopResultado());
    }

    // ------------------------------------------------------------------ Construção da UI

    private void ConstruirUI()
    {
        var cgo = new GameObject("BochinhaHUD_Canvas", typeof(Canvas), typeof(CanvasScaler));
        cgo.transform.SetParent(transform, false);
        canvas = cgo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = ordemCanvas;

        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 600f); // ESCALADO PARA 800x600
        scaler.matchWidthOrHeight = 0.5f;

        Transform raiz = cgo.transform;

        // Vinheta vermelha (tempo acabando)
        vinheta = CriarImagem("Vinheta", raiz, new Color(1f, 0f, 0f, 0f), Vector2.zero, Vector2.one);
        vinheta.sprite = CriarSpriteVinheta();

        // Turno (topo) - Reduzido pos, tam e fonte
        CriarImagemFixa("FundoTurno", raiz, null, new Color(0f, 0f, 0f, 0.45f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(500f, 45f));
        txtTurno = CriarTexto("Turno", raiz, 32f, Color.white, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(500f, 45f), TextAlignmentOptions.Center);
        rtTurno = txtTurno.rectTransform;

        // Aviso de tempo (logo abaixo do turno) - Reduzido pos, tam e fonte
        txtTempo = CriarTexto("Tempo", raiz, 28f, new Color(1f, 0.25f, 0.2f), new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(550f, 45f), TextAlignmentOptions.Center);
        txtTempo.text = avisoTempo;
        txtTempo.alpha = 0f;

        // Rótulos dos contadores - Reduzido pos, tam e fonte
        var rotJ = CriarTexto("RotuloJogador", raiz, 18f, corJogador, new Vector2(0f, 0f), new Vector2(25f, 65f), new Vector2(180f, 28f), TextAlignmentOptions.Left);
        rotJ.text = "VOCE";
        var rotA = CriarTexto("RotuloAdversario", raiz, 18f, corAdversario, new Vector2(1f, 0f), new Vector2(-25f, 65f), new Vector2(180f, 28f), TextAlignmentOptions.Right);
        rotA.text = "ADVERSARIO";

        // Dica (embaixo, centro) - Reduzido pos, tam e fonte
        txtDica = CriarTexto("Dica", raiz, 26f, Color.white, new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(750f, 45f), TextAlignmentOptions.Center);
        rtDica = txtDica.rectTransform;
        txtDica.alpha = 0f;

        // Barra de força (aparece só durante o arraste) - Reduzido pos, tam e fonte
        var barra = new GameObject("BarraForca", typeof(RectTransform));
        barra.transform.SetParent(raiz, false);
        var rtBarra = (RectTransform)barra.transform;
        rtBarra.anchorMin = rtBarra.anchorMax = new Vector2(0.5f, 0f);
        rtBarra.pivot = new Vector2(0.5f, 0.5f);
        rtBarra.anchoredPosition = new Vector2(0f, 40f);
        rtBarra.sizeDelta = new Vector2(360f, 28f);
        grupoForca = barra;

        CriarImagem("Borda", barra.transform, new Color(0f, 0f, 0f, 0.9f), Vector2.zero, Vector2.one);
        var fundo = CriarImagem("Fundo", barra.transform, new Color(0.15f, 0.12f, 0.2f, 1f), Vector2.zero, Vector2.one);
        fundo.rectTransform.offsetMin = new Vector2(3f, 3f);
        fundo.rectTransform.offsetMax = new Vector2(-3f, -3f);
        forcaFill = CriarImagem("Fill", fundo.transform, new Color(0.35f, 0.9f, 0.3f), Vector2.zero, new Vector2(0f, 1f));
        txtForca = CriarTexto("TextoForca", barra.transform, 16f, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360f, 28f), TextAlignmentOptions.Center);
        barra.SetActive(false);

        // Intro (centro) - Reduzido pos, tam e fonte
        var intro = new GameObject("Intro", typeof(RectTransform), typeof(CanvasGroup));
        intro.transform.SetParent(raiz, false);
        rtIntro = (RectTransform)intro.transform;
        rtIntro.anchorMin = rtIntro.anchorMax = new Vector2(0.5f, 0.5f);
        rtIntro.sizeDelta = new Vector2(750f, 180f);
        rtIntro.anchoredPosition = new Vector2(0f, 25f);
        grupoIntro = intro.GetComponent<CanvasGroup>();
        grupoIntro.blocksRaycasts = false;

        txtIntro = CriarTexto("Titulo", intro.transform, 64f, new Color(1f, 0.9f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(750f, 85f), TextAlignmentOptions.Center);
        txtIntro.text = tituloIntro;
        txtSub = CriarTexto("Subtitulo", intro.transform, 24f, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(750f, 45f), TextAlignmentOptions.Center);
        txtSub.text = subtituloIntro;

        // Resultado (parte de cima do centro, pra não cobrir as bolas) - Reduzido pos, tam e fonte
        txtResultado = CriarTexto("Resultado", raiz, 68f, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(800f, 100f), TextAlignmentOptions.Center);
        rtResultado = txtResultado.rectTransform;
        txtResultado.gameObject.SetActive(false);
    }

    private Image CriarImagem(string nome, Transform pai, Color cor, Vector2 ancoraMin, Vector2 ancoraMax)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = ancoraMin;
        rt.anchorMax = ancoraMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = cor;
        img.raycastTarget = false;
        return img;
    }

    private Image CriarImagemFixa(string nome, Transform pai, Sprite sprite, Color cor, Vector2 ancora, Vector2 pos, Vector2 tam)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = ancora;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = cor;
        img.raycastTarget = false;
        return img;
    }

    private TextMeshProUGUI CriarTexto(string nome, Transform pai, float tamanho, Color cor, Vector2 ancora, Vector2 pos, Vector2 tamRect, TextAlignmentOptions alinhamento)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = ancora;

        float pivotX = 0.5f;
        if (alinhamento == TextAlignmentOptions.Left) pivotX = 0f;
        else if (alinhamento == TextAlignmentOptions.Right) pivotX = 1f;
        rt.pivot = new Vector2(pivotX, 0.5f);

        rt.anchoredPosition = pos;
        rt.sizeDelta = tamRect;

        var t = go.AddComponent<TextMeshProUGUI>();
        t.alignment = alinhamento;
        t.fontSize = tamanho;
        t.fontStyle = FontStyles.Bold;
        t.color = cor;
        t.outlineWidth = 0.3f;
        t.outlineColor = new Color32(0, 0, 0, 255);
        t.raycastTarget = false;
        return t;
    }

    private Sprite CriarSpriteCirculo()
    {
        const int s = 64;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = (x + 0.5f) / s * 2f - 1f;
                float dy = (y + 0.5f) / s * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, 1f - Mathf.SmoothStep(0.88f, 1f, d)));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f));
    }

    private Sprite CriarSpriteVinheta()
    {
        const int s = 128;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = x / (s - 1f) * 2f - 1f;
                float dy = y / (s - 1f) * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.SmoothStep(0.55f, 1.25f, d)));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f));
    }

    // ------------------------------------------------------------------ Contadores de bochas

    private void GarantirPontos(List<Image> lista, int total, Vector2 ancora, int lado, string nome)
    {
        if (lista.Count == total) return;

        foreach (var p in lista) if (p != null) Destroy(p.gameObject);
        lista.Clear();

        for (int i = 0; i < total; i++)
        {
            // Diminuído o raio das bolas de vida e o espaçamento (75f/70f -> 35f/32f)
            Vector2 pos = new Vector2(lado * (35f + i * 32f), 35f);
            lista.Add(CriarImagemFixa(nome + i, canvas.transform, spriteCirculo, Color.white, ancora, pos, new Vector2(24f, 24f)));
        }
    }

    private void PintarPontos(List<Image> lista, int restantes, Color cor)
    {
        for (int i = 0; i < lista.Count; i++)
            lista[i].color = i < restantes ? cor : new Color(1f, 1f, 1f, 0.18f);
    }

    // ------------------------------------------------------------------ Animações

    private static float EaseOutBack(float k)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
    }

    private IEnumerator Intro()
    {
        grupoIntro.alpha = 1f;

        float t = 0f;
        const float pop = 0.35f;
        while (t < pop)
        {
            t += Time.deltaTime;
            rtIntro.localScale = Vector3.one * Mathf.Max(0.01f, EaseOutBack(Mathf.Clamp01(t / pop)));
            yield return null;
        }
        rtIntro.localScale = Vector3.one;

        yield return new WaitForSeconds(duracaoIntro);

        float f = 0f;
        const float fade = 0.3f;
        while (f < fade)
        {
            f += Time.deltaTime;
            grupoIntro.alpha = 1f - Mathf.Clamp01(f / fade);
            yield return null;
        }
        rtIntro.gameObject.SetActive(false);
    }

    private IEnumerator PopTurno()
    {
        float t = 0f;
        const float dur = 0.25f;
        while (t < dur)
        {
            t += Time.deltaTime;
            rtTurno.localScale = Vector3.one * Mathf.LerpUnclamped(0.7f, 1f, EaseOutBack(Mathf.Clamp01(t / dur)));
            yield return null;
        }
        rtTurno.localScale = Vector3.one;
    }

    private IEnumerator PopResultado()
    {
        float t = 0f;
        const float dur = 0.4f;
        while (t < dur)
        {
            t += Time.deltaTime;
            rtResultado.localScale = Vector3.one * Mathf.Max(0.01f, EaseOutBack(Mathf.Clamp01(t / dur)));
            yield return null;
        }
        rtResultado.localScale = Vector3.one;
    }

    // ------------------------------------------------------------------ Loop

    void Update()
    {
        if (canvas == null) return;

        BochinhaLauncher l = BochinhaLauncher.Instance;
        bool pode = l != null && l.PodeLancar;
        bool arrastando = l != null && l.Arrastando;

        if (finalizado)
        {
            txtDica.alpha = 0f;
            txtTempo.alpha = 0f;
            grupoForca.SetActive(false);

            var cf = vinheta.color;
            cf.a = Mathf.MoveTowards(cf.a, derrotaPorTempo ? 0.55f : 0f, Time.deltaTime * 2f);
            vinheta.color = cf;
            return;
        }

        // Dica de como lancar
        if (dicaCurtaTimer > 0f) dicaCurtaTimer -= Time.deltaTime;
        bool bolim = gm != null && gm.AguardandoBolim;
        txtDica.text = dicaCurtaTimer > 0f ? dicaArrasteCurto : (bolim ? dicaBolim : dicaBocha);
        txtDica.alpha = (pode && !arrastando) ? 1f : 0f;
        rtDica.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(Time.time * 7f));

        // Barra de forca durante o arraste
        grupoForca.SetActive(arrastando);
        if (arrastando)
        {
            float p = l.PoderNormalizado;
            forcaFill.rectTransform.anchorMax = new Vector2(p, 1f);
            Color verde = new Color(0.35f, 0.9f, 0.3f);
            Color amarelo = new Color(1f, 0.9f, 0.2f);
            Color vermelho = new Color(1f, 0.25f, 0.2f);
            forcaFill.color = p < 0.5f ? Color.Lerp(verde, amarelo, p * 2f) : Color.Lerp(amarelo, vermelho, (p - 0.5f) * 2f);
            txtForca.text = "FORCA " + Mathf.RoundToInt(p * 100f) + "%";
        }

        // Tempo acabando: vinheta pulsando + aviso piscando, mais rapido quanto mais perto do fim
        float tn = gm != null ? gm.TempoNormalizado : 0f;
        float perigo = Mathf.InverseLerp(tempoAcabandoEm, 1f, tn);

        var cv = vinheta.color;
        cv.a = perigo * (0.35f + 0.25f * Mathf.Sin(Time.time * (4f + 8f * perigo)));
        vinheta.color = cv;

        if (perigo > 0f)
            txtTempo.alpha = Mathf.Sin(Time.time * (6f + 10f * perigo)) > 0f ? 1f : 0.25f;
        else
            txtTempo.alpha = 0f;
    }
}