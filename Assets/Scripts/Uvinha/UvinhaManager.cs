using System.Collections;
using UnityEngine;

public class UvinhaManager : MinigameBase
{
    public static UvinhaManager Instance;

    [Header("Referências")]
    public Transform tesouraTransform;
    public Transform startPoint;
    public Transform cutPoint; // só é usado se "terminarOndeATesouraEstaNaCena" estiver desmarcado
    public ShakeController uvaController;
    public UvinhaHUD hud; // se vazio, é criado automaticamente

    [Header("Tesoura")]
    // Marcado: a tesoura termina a caminhada EXATAMENTE onde você deixou ela na cena (posição do Transform no editor).
    // Isso resolve o problema do pivot: o cutPoint posicionava o centro do sprite, e a tesoura passava do ponto.
    public bool terminarOndeATesouraEstaNaCena = true;
    public AnimationCurve curvaDeAproximacao = AnimationCurve.Linear(0f, 0f, 1f, 1f); // linear = velocidade constante
    public float pulinhoAltura = 0.06f;      // "caminhando": pulinhos que somem no fim
    public float pulinhoFrequencia = 9f;

    [Header("Tempo")]
    public float tempoMinimo = 7f;           // o GameManager reduz o tempo a cada fase; aqui ele nunca fica abaixo disso
    public float tempoPadraoTeste = 8f;      // usado se a cena rodar sozinha

    private float tempoDaRodada;
    private float elapsedTime = 0f;
    private bool rodadaEncerrada = false;    // só usado no modo teste (sem GameManagerRework)
    private bool referenciasOk = false;
    private Vector3 posInicio, posFim;

    /// <summary>0 = tesoura no spawn, 1 = tesoura no ponto final. Usado pelo HUD e pela uva.</summary>
    public float TesouraProgresso { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (tempoDaRodada <= 0f) tempoDaRodada = tempoPadraoTeste;

        if (hud == null) hud = FindObjectOfType<UvinhaHUD>();
        if (hud == null) hud = gameObject.AddComponent<UvinhaHUD>();

        if (tesouraTransform == null || startPoint == null || (!terminarOndeATesouraEstaNaCena && cutPoint == null))
        {
            Debug.LogError("[Uvinha] tesouraTransform, startPoint (ou cutPoint) não está atribuído no Inspector!", this);
            return;
        }

        // Lê as posições ANTES de mexer na tesoura
        posFim = terminarOndeATesouraEstaNaCena ? tesouraTransform.position : cutPoint.position;
        posInicio = startPoint.position;
        tesouraTransform.position = posInicio; // a tesoura nasce no spawn point
        referenciasOk = true;
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // O valor retornado vira o timer do GameManager, então a tesoura chega junto com o fim do tempo.
        tempoDaRodada = Mathf.Max(tempoMinimo, tempoGlobalSugerido);
        return tempoDaRodada;
    }

    void Update()
    {
        if (jogoFinalizado || rodadaEncerrada) return;

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / tempoDaRodada);
        TesouraProgresso = Mathf.Clamp01(curvaDeAproximacao.Evaluate(t));
        MoverTesoura(TesouraProgresso);

        // A tesoura chegou na uva = derrota. Não depende do timer do GameManager,
        // então funciona até rodando a cena sozinha (o guard de jogoFinalizado evita chamar duas vezes).
        if (t >= 1f) TempoEsgotado();
    }

    private void MoverTesoura(float k)
    {
        if (!referenciasOk) return;

        Vector3 pos = Vector3.Lerp(posInicio, posFim, k);
        pos.y += Mathf.Abs(Mathf.Sin(Time.time * pulinhoFrequencia)) * pulinhoAltura * (1f - k);
        tesouraTransform.position = pos;
    }

    // Método chamado pelo ShakeController quando a barra chega a 100%
    public void UvaEscapou()
    {
        if (jogoFinalizado || rodadaEncerrada) return;

        // Feedback antes do Vencer(), pra aparecer mesmo se algo falhar no GameManager
        if (hud != null) hud.MostrarResultado(true);
        StartCoroutine(RecuarTesoura());

        if (GameManagerRework.Instance != null) Vencer();
        else EncerrarModoTeste("VITÓRIA");
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado || rodadaEncerrada) return;

        // A tesoura chegou à uva!
        TesouraProgresso = 1f;
        MoverTesoura(1f);
        if (uvaController != null) uvaController.TriggerLose();
        if (hud != null) hud.MostrarResultado(false);

        if (GameManagerRework.Instance != null) base.TempoEsgotado(); // Chama o Perder() interno
        else EncerrarModoTeste("DERROTA");
    }

    // Sem GameManagerRework (cena rodando sozinha), Vencer()/Perder() dariam NullReferenceException
    private void EncerrarModoTeste(string resultado)
    {
        rodadaEncerrada = true;
        Debug.Log("[Uvinha] " + resultado + " (modo teste: sem GameManagerRework na cena. Pare e dê Play de novo, ou comece pela cena Menu.)");
    }

    // A tesoura desiste e sai de cena quando a uva escapa
    private IEnumerator RecuarTesoura()
    {
        if (!referenciasOk) yield break;

        Vector3 de = tesouraTransform.position;
        float t = 0f;
        const float dur = 0.6f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            tesouraTransform.position = Vector3.Lerp(de, posInicio, k * k);
            yield return null;
        }
    }
}