using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD do minigame da Uvinha, 100% criado por código (não precisa montar Canvas nem arrastar nada).
/// O UvinhaManager adiciona este componente sozinho se ele não existir na cena.
/// Mostra: intro "LIBERTE A UVINHA!", dica pulsante, barra de liberdade,
/// alerta vermelho conforme a tesoura se aproxima e o resultado (ESCAPOU! / SNIP!).
/// Obs: os textos estão sem acento de propósito (a fonte padrão do TMP pode não ter os glifos).
/// </summary>
public class UvinhaHUD : MonoBehaviour
{
    [Header("Textos")]
    public string tituloIntro = "LIBERTE A UVINHA!";
    public string subtituloIntro = "Escape antes que a tesoura chegue!";
    public string dicaParada = "AGITE O CELULAR!";
    public string dicaEditor = " (ou segure ESPACO)";
    public string dicaAgitando = "ISSO! CONTINUA!";
    public string avisoPerigo = "CUIDADO! A TESOURA!";
    public string textoVitoria = "ESCAPOU!";
    public string textoDerrota = "SNIP!";

    [Header("Ajustes")]
    public int ordemCanvas = 20;          // aumente se o CanvasHUD da TelaIntervalo ficar por cima
    public float duracaoIntro = 1.4f;     // quanto tempo o aviso inicial fica parado na tela
    [Range(0f, 1f)] public float perigoComecaEm = 0.45f; // a partir de quanto da trajetoria da tesoura o alerta aparece

    private UvinhaManager manager;
    private ShakeController uva;

    private Canvas canvas;
    private Image vinheta, barraFill;
    private TextMeshProUGUI txtIntro, txtSub, txtDica, txtPerigo, txtResultado, txtBarra;
    private RectTransform rtIntro, rtDica, rtResultado;
    private CanvasGroup grupoIntro;
    private bool finalizado;
    private bool venceu;

    void Start()
    {
        manager = UvinhaManager.Instance != null ? UvinhaManager.Instance : GetComponent<UvinhaManager>();
        uva = manager != null && manager.uvaController != null ? manager.uvaController : FindObjectOfType<ShakeController>();

        ConstruirUI();
        StartCoroutine(Intro());
    }

    // ------------------------------------------------------------------ UI

    private void ConstruirUI()
    {
        var cgo = new GameObject("UvinhaHUD_Canvas", typeof(Canvas), typeof(CanvasScaler));
        cgo.transform.SetParent(transform, false);
        canvas = cgo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = ordemCanvas;

        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Transform raiz = cgo.transform;

        // Vinheta vermelha (alerta de perigo)
        vinheta = CriarImagem("Vinheta", raiz, new Color(1f, 0f, 0f, 0f), Vector2.zero, Vector2.one);
        vinheta.sprite = CriarSpriteVinheta();

        // Aviso de perigo (topo)
        txtPerigo = CriarTexto("Perigo", raiz, 72f, new Color(1f, 0.25f, 0.2f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1400f, 120f));
        txtPerigo.text = avisoPerigo;
        txtPerigo.alpha = 0f;

        // Barra de liberdade (embaixo)
        var barra = new GameObject("BarraLiberdade", typeof(RectTransform));
        barra.transform.SetParent(raiz, false);
        var rtBarra = (RectTransform)barra.transform;
        rtBarra.anchorMin = rtBarra.anchorMax = new Vector2(0.5f, 0f);
        rtBarra.pivot = new Vector2(0.5f, 0.5f);
        rtBarra.anchoredPosition = new Vector2(0f, 110f);
        rtBarra.sizeDelta = new Vector2(900f, 64f);

        CriarImagem("Borda", barra.transform, new Color(0f, 0f, 0f, 0.9f), Vector2.zero, Vector2.one);
        var fundo = CriarImagem("Fundo", barra.transform, new Color(0.15f, 0.12f, 0.2f, 1f), Vector2.zero, Vector2.one);
        fundo.rectTransform.offsetMin = new Vector2(6f, 6f);
        fundo.rectTransform.offsetMax = new Vector2(-6f, -6f);
        barraFill = CriarImagem("Fill", fundo.transform, new Color(1f, 0.55f, 0.1f), Vector2.zero, new Vector2(0f, 1f));

        txtBarra = CriarTexto("TextoBarra", barra.transform, 36f, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 64f));
        txtBarra.text = "LIBERDADE 0%";

        // Dica pulsante (acima da barra)
        txtDica = CriarTexto("Dica", raiz, 64f, Color.white, new Vector2(0.5f, 0f), new Vector2(0f, 215f), new Vector2(1600f, 110f));
        rtDica = txtDica.rectTransform;

        // Intro (centro)
        var intro = new GameObject("Intro", typeof(RectTransform), typeof(CanvasGroup));
        intro.transform.SetParent(raiz, false);
        rtIntro = (RectTransform)intro.transform;
        rtIntro.anchorMin = rtIntro.anchorMax = new Vector2(0.5f, 0.5f);
        rtIntro.sizeDelta = new Vector2(1700f, 400f);
        rtIntro.anchoredPosition = new Vector2(0f, 90f);
        grupoIntro = intro.GetComponent<CanvasGroup>();
        grupoIntro.blocksRaycasts = false;

        txtIntro = CriarTexto("Titulo", intro.transform, 130f, new Color(1f, 0.9f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(0f, 50f), new Vector2(1700f, 180f));
        txtIntro.text = tituloIntro;
        txtSub = CriarTexto("Subtitulo", intro.transform, 54f, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(1700f, 100f));
        txtSub.text = subtituloIntro;

        // Resultado (centro)
        txtResultado = CriarTexto("Resultado", raiz, 200f, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1700f, 300f));
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

    private TextMeshProUGUI CriarTexto(string nome, Transform pai, float tamanho, Color cor, Vector2 ancora, Vector2 pos, Vector2 tamRect)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = ancora;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tamRect;

        var t = go.AddComponent<TextMeshProUGUI>();
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = tamanho;
        t.fontStyle = FontStyles.Bold;
        t.color = cor;
        t.outlineWidth = 0.3f;
        t.outlineColor = new Color32(0, 0, 0, 255);
        t.raycastTarget = false;
        return t;
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

    /// <summary>Chamado pelo UvinhaManager quando a rodada termina.</summary>
    public void MostrarResultado(bool ganhou)
    {
        if (finalizado || txtResultado == null) return;
        finalizado = true;
        venceu = ganhou;

        txtResultado.text = ganhou ? textoVitoria : textoDerrota;
        txtResultado.color = ganhou ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.3f, 0.25f);
        txtResultado.gameObject.SetActive(true);
        if (rtIntro != null) rtIntro.gameObject.SetActive(false);
        StartCoroutine(PopResultado());
    }

    // ------------------------------------------------------------------ Loop

    void Update()
    {
        if (canvas == null) return;

        float p = uva != null ? uva.Progress01 : 0f;
        bool agitando = uva != null && uva.Agitando;
        float tesoura = manager != null ? manager.TesouraProgresso : 0f;
        float perigo = Mathf.InverseLerp(perigoComecaEm, 1f, tesoura);

        // Barra de liberdade
        barraFill.rectTransform.anchorMax = new Vector2(p, 1f);
        barraFill.color = Color.Lerp(new Color(1f, 0.55f, 0.1f), new Color(0.35f, 0.9f, 0.3f), p);
        txtBarra.text = "LIBERDADE " + Mathf.RoundToInt(p * 100f) + "%";

        if (finalizado)
        {
            txtDica.alpha = 0f;
            txtPerigo.alpha = 0f;
            // vitoria: some o alerta; derrota: fica o clarao vermelho
            float alvo = venceu ? 0f : 0.55f;
            var c = vinheta.color;
            c.a = Mathf.MoveTowards(c.a, alvo, Time.deltaTime * 2f);
            vinheta.color = c;
            return;
        }

        // Dica pulsante
        string dica = Application.isEditor ? dicaParada + dicaEditor : dicaParada;
        txtDica.text = agitando ? dicaAgitando : dica;
        txtDica.color = agitando ? new Color(0.4f, 1f, 0.4f) : Color.white;
        float pulso = 1f + (agitando ? 0.12f : 0.07f) * Mathf.Sin(Time.time * (agitando ? 16f : 7f));
        rtDica.localScale = Vector3.one * pulso;

        // Perigo: vinheta pulsando + texto piscando, mais rapido quanto mais perto
        var cv = vinheta.color;
        cv.a = perigo * (0.35f + 0.25f * Mathf.Sin(Time.time * (4f + 8f * perigo)));
        vinheta.color = cv;

        if (perigo > 0f)
        {
            bool aceso = Mathf.Sin(Time.time * (6f + 10f * perigo)) > 0f;
            txtPerigo.alpha = aceso ? 1f : 0.25f;
        }
        else
        {
            txtPerigo.alpha = 0f;
        }
    }
}