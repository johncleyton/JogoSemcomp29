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
    [Tooltip("Segundos sem nenhum sopro acima do threshold até liberar o espaço (decisão única)")]
    public float tempoParaLiberarEspaco = 1f;
    [Tooltip("Quantas rodadas seguidas sem sopro são necessárias para lembrar que não há microfone. 1 = lembra já na primeira")]
    public int rodadasSemSoproParaConfirmar = 1;

    [Header("Dica (só quando o espaço está liberado)")]
    public GameObject dicaEspaco;
    public float intervaloPiscar = 0.4f;

    [Header("Animators")]
    public Animator player;
    public Animator vacas;

    [Header("Timers de animação")]
    public float delayAnimacaoLose = 1f;
    public float delayAnimacaoWin = 1f;

    private static bool semMicConfirmado = false;
    private static int rodadasSemSopro = 0;

    private float currentFill = 0f;
    private float margemIndicador;

    private bool modoDecidido = false;
    private bool usandoFallback = false;
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

        if (forcarFallback || detector == null || !temMic || semMicConfirmado)
        {
            usandoFallback = true;
            modoDecidido = true;
        }

        if (dicaEspaco != null)
            dicaEspaco.SetActive(false);
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

        bool micSoprando = false;
        if (!usandoFallback && detector != null)
        {
            float loudness = detector.getLoudnessMic() * audioSens;
            micSoprando = loudness >= threshold;
        }

        if (!modoDecidido)
        {
            if (micSoprando)
            {
                modoDecidido = true;
                rodadasSemSopro = 0;
            }
            else
            {
                tempoObservando += Time.deltaTime;
                if (tempoObservando >= tempoParaLiberarEspaco)
                {
                    usandoFallback = true;
                    modoDecidido = true;

                    rodadasSemSopro++;
                    if (rodadasSemSopro >= rodadasSemSoproParaConfirmar)
                        semMicConfirmado = true;
                }
            }
        }

        AtualizarDica();

        bool soprando = false;

        if (usandoFallback)
        {
            if (Input.GetKeyDown(fallbackKey))
            {
                currentFill += fallbackFillPerPress;
                soprando = true;
            }
            else
            {
                currentFill -= drainSpeed * Time.deltaTime;
            }
        }
        else
        {
            if (micSoprando)
            {
                currentFill += fillSpeed * Time.deltaTime;
                soprando = true;
            }
            else
            {
                currentFill -= drainSpeed * Time.deltaTime;
            }
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

    private void AtualizarDica()
    {
        if (dicaEspaco == null) return;

        if (!usandoFallback)
        {
            dicaEspaco.SetActive(false);
            return;
        }

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