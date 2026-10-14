using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Coloque este componente em um objeto raiz da cena de intervalo.
// O Canvas e a RawImage da transicao sao criados automaticamente.
public class TransicaoIris : MonoBehaviour
{
    [Header("Material com o shader UI/TransicaoIris")]
    [SerializeField] private Material materialIris;

    [Header("Duracao em segundos")]
    [Min(0.01f)] public float duracaoAbertura = 0.45f;
    [Min(0.01f)] public float duracaoFechamento = 0.4f;

    [Header("Borda do circulo")]
    [Range(0.001f, 0.03f)] public float suavidade = 0.003f;
    

    public bool EmTransicao { get; private set; }
    public bool EstaPronta => materialInstancia != null;

    private Material materialInstancia;
    private RawImage imagem;
    private RenderTexture captura;
    private float escalaTempoAnterior;
    private bool tempoSuspenso;
    private bool mostrandoDentro;

    private static readonly int RaioId = Shader.PropertyToID("_Raio");
    private static readonly int AspectoId = Shader.PropertyToID("_Aspecto");
    private static readonly int SuavidadeId = Shader.PropertyToID("_Suavidade");
    private static readonly int DentroId = Shader.PropertyToID("_MostrarDentro");

    private float RaioMinimo => -2f * Mathf.Max(suavidade, 0.001f);
    [SerializeField] private bool inverterCapturaVerticalmente = true;
    private float RaioMaximo
    {
        get
        {
            float aspecto = (float)Screen.width / Mathf.Max(1, Screen.height);
            // Atinge os cantos da tela, inclusive em telas largas.
            return 0.5f * Mathf.Sqrt(aspecto * aspecto + 1f)
                + 2f * Mathf.Max(suavidade, 0.001f);
        }
    }

    private void Awake()
    {
        if (materialIris == null || !materialIris.HasProperty(RaioId)
            || !materialIris.HasProperty(DentroId))
        {
            Debug.LogError("TransicaoIris: atribua um material com o shader UI/TransicaoIris.", this);
            return;
        }

        materialInstancia = new Material(materialIris);

        GameObject objetoCanvas = new GameObject("CanvasIris",
            typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        objetoCanvas.transform.SetParent(transform, false);

        Canvas canvas = objetoCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        GameObject objetoImagem = new GameObject("CapturaDaTela",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        objetoImagem.transform.SetParent(objetoCanvas.transform, false);

        imagem = objetoImagem.GetComponent<RawImage>();
        imagem.material = materialInstancia;
        imagem.color = Color.white;
        imagem.raycastTarget = true;
        imagem.maskable = false;
        imagem.enabled = false;

        RectTransform rect = imagem.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // A captura do intervalo cobre a tela; depois abrimos um buraco nela.
    public IEnumerator PrepararAbertura()
    {
        yield return PrepararCaptura(false);
    }

    // A captura do minigame cobre a tela; depois encolhemos seu recorte.
    public IEnumerator PrepararFechamento()
    {
        yield return PrepararCaptura(true);
    }

    private IEnumerator PrepararCaptura(bool mostrarDentro)
    {
        if (!EstaPronta)
            throw new System.InvalidOperationException("Configure o material da TransicaoIris.");
        if (EmTransicao)
            throw new System.InvalidOperationException("Ja existe uma transicao em andamento.");

        EmTransicao = true;
        escalaTempoAnterior = Time.timeScale;
        tempoSuspenso = true;
        Time.timeScale = 0f;

        imagem.enabled = false;
        // Inclui cameras, sprites e a UI, sem capturar a propria sobreposicao.
        yield return new WaitForEndOfFrame();

        int largura = Mathf.Max(1, Screen.width);
        int altura = Mathf.Max(1, Screen.height);
        if (captura == null || captura.width != largura || captura.height != altura)
        {
            LiberarCaptura();
            captura = new RenderTexture(largura, altura, 0, RenderTextureFormat.ARGB32);
            captura.name = "CapturaTransicaoIris";
            captura.filterMode = FilterMode.Bilinear;
            captura.wrapMode = TextureWrapMode.Clamp;
            captura.Create();
        }

        ScreenCapture.CaptureScreenshotIntoRenderTexture(captura);
        imagem.texture = captura;

        imagem.uvRect = inverterCapturaVerticalmente
            ? new Rect(0f, 1f, 1f, -1f)
            : new Rect(0f, 0f, 1f, 1f);
        mostrandoDentro = mostrarDentro;
        materialInstancia.SetFloat(DentroId, mostrarDentro ? 1f : 0f);
        DefinirRaio(mostrarDentro ? RaioMaximo : RaioMinimo);
        imagem.enabled = true;
    }

    public IEnumerator Abrir()
    {
        if (!EmTransicao || mostrandoDentro)
            throw new System.InvalidOperationException("Chame PrepararAbertura antes de Abrir.");
        yield return Animar(true, duracaoAbertura);
    }

    public IEnumerator Fechar()
    {
        if (!EmTransicao || !mostrandoDentro)
            throw new System.InvalidOperationException("Chame PrepararFechamento antes de Fechar.");
        yield return Animar(false, duracaoFechamento);
    }

    private IEnumerator Animar(bool abrindo, float duracao)
    {
        // Da um quadro a cena de destino para preparar layout e renderizacao.
        yield return null;
        float tempo = 0f;
        duracao = Mathf.Max(0.01f, duracao);

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tempo / duracao));
            float raio = abrindo
                ? Mathf.Lerp(RaioMinimo, RaioMaximo, t)
                : Mathf.Lerp(RaioMaximo, RaioMinimo, t);
            DefinirRaio(raio);
            yield return null;
        }

        imagem.enabled = false;
        RestaurarTempo();
        EmTransicao = false;
    }

    private void DefinirRaio(float raio)
    {
        materialInstancia.SetFloat(RaioId, raio);
        materialInstancia.SetFloat(AspectoId,
            (float)Screen.width / Mathf.Max(1, Screen.height));
        materialInstancia.SetFloat(SuavidadeId, Mathf.Max(suavidade, 0.001f));
    }

    private void RestaurarTempo()
    {
        if (!tempoSuspenso)
            return;
        Time.timeScale = escalaTempoAnterior;
        tempoSuspenso = false;
    }

    private void LiberarCaptura()
    {
        if (captura == null)
            return;
        if (imagem != null)
            imagem.texture = null;
        captura.Release();
        Destroy(captura);
        captura = null;
    }

    private void OnDestroy()
    {
        RestaurarTempo();
        LiberarCaptura();
        if (materialInstancia != null)
            Destroy(materialInstancia);
    }
}
