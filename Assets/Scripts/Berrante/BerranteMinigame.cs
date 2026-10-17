using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class BerranteMinigame : MinigameBase
{
    [Header("Referências")]
    public DetectorAudio detector;
    public RectTransform barra;
    public RectTransform indicador;

    [Header("Configuração de áudio")]
    public float audioSens = 5f;
    public float threshold = 0.15f;

    [Header("Configuração do minigame")]
    public float requiredFill = 1f;
    public float fillSpeed = 0.5f;
    public float drainSpeed = 0.3f;
    public float timeLimit = 4f;

    [Header("Configuração da barra (visual)")]
    public float paddingEsquerda = 10f;
    public float paddingDireita = 10f;

    [Header("Fallback sem microfone")]
    public bool forcarFallback = false;
    public KeyCode fallbackKey = KeyCode.Space;
    public float fallbackFillPerPress = 0.08f;

    [Header("Detecção de microfone por sinal (funciona em qualquer build)")]
    [Tooltip("Segundos sem NENHUM sinal de áudio para considerar que não há microfone")]
    public float tempoVerificacaoMic = 1f;
    [Tooltip("Loudness acima disso conta como 'microfone vivo'")]
    public float limiteSinalMinimo = 0.00001f;

    [Header("Dica (só sem microfone)")]
    public GameObject dicaEspaco;
    public float intervaloPiscar = 0.4f;

    [Header("Animators")]
    public Animator player;
    public Animator vacas;

    [Header("Timers de animação")]
    public float delayAnimacaoLose = 1f;
    public float delayAnimacaoWin = 1f;

    private float currentFill = 0f;
    private bool usandoFallback = false;
    private float margemIndicador;

    private bool micTemSinal = false;
    private float tempoObservando = 0f;

    void Start()
    {
        Canvas.ForceUpdateCanvases();

        if (indicador != null)
            margemIndicador = indicador.rect.width * 0.5f;

        AtualizarIndicador(0f);

        bool temMic = true;

#if !UNITY_WEBGL || UNITY_EDITOR
        temMic = Microphone.devices.Length > 0;
#endif

        usandoFallback = forcarFallback || detector == null || !temMic;

        if (dicaEspaco != null)
            dicaEspaco.SetActive(usandoFallback);
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        return timeLimit > 0 ? timeLimit : tempoGlobalSugerido;
    }

    void Update()
    {
        if (jogoFinalizado)
        {
            if (dicaEspaco != null)
                dicaEspaco.SetActive(false);
            return;
        }

        VerificarMicrofonePorSinal();

        AtualizarDica();

        bool soprando;
        if (usandoFallback)
        {
            if (Input.GetKeyDown(fallbackKey))
            {
                currentFill += fallbackFillPerPress;
                soprando = true;
            }
            else
            {
                soprando = false;
                currentFill -= drainSpeed * Time.deltaTime;
            }
        }
        else
        {
            float loudness = detector.getLoudnessMic() * audioSens;
            soprando = loudness >= threshold;
            if (soprando)
                currentFill += fillSpeed * Time.deltaTime;
            else
                currentFill -= drainSpeed * Time.deltaTime;
        }

        if (soprando)
            player.SetTrigger("Soprando");

        currentFill = Mathf.Clamp01(currentFill);
        AtualizarIndicador(currentFill);

        if (currentFill >= requiredFill)
        {
            vacas.SetTrigger("Win");
            player.SetTrigger("Win");

            VencerComAtraso(delayAnimacaoWin);
        }
    }

    private void VerificarMicrofonePorSinal()
    {
        if (usandoFallback || micTemSinal) return;

        float bruto = detector.getLoudnessMic();
        if (bruto > limiteSinalMinimo)
        {
            micTemSinal = true;
            return;
        }

        tempoObservando += Time.deltaTime;
        if (tempoObservando >= tempoVerificacaoMic)
            usandoFallback = true;
    }

    private void AtualizarDica()
    {
        if (!usandoFallback || dicaEspaco == null) return;

        bool visivel = Mathf.FloorToInt(Time.time / intervaloPiscar) % 2 == 0;
        dicaEspaco.SetActive(visivel);
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;

        player.SetTrigger("Lose");

        PerderComAtraso(delayAnimacaoLose);
    }

    private void AtualizarIndicador(float progresso)
    {
        if (indicador == null || barra == null) return;

        float larguraBarra = barra.rect.width;
        float margemBase = Mathf.Min(margemIndicador, larguraBarra * 0.5f);

        float esquerda = margemBase + paddingEsquerda;
        float direita = larguraBarra - margemBase - paddingDireita;

        float x = Mathf.Lerp(esquerda, direita, progresso);

        Vector2 pos = indicador.anchoredPosition;
        pos.x = x;
        indicador.anchoredPosition = pos;
    }
}