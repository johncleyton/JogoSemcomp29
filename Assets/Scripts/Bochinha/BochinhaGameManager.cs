using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; // textmeshpro
public class BochinhaGameManager : MinigameBase
{
    public static BochinhaGameManager Instance;

    // Estados possíveis de uma rodada de bochinha (1 jogador vs CPU)
    private enum EstadoRodada { AguardandoBolim, TurnoJogador, TurnoCPU, Finalizado }

    [Header("Configurações do Jogo")]
    public int bochasPorTime = 2; // cada lance leva ~6s (mira + bola parar), então 2 cabe no tempo; 4 não cabe
    public Transform pontoDeLancamento; // de onde a CPU lança (ideal: lado oposto ao launchPoint do jogador)
    public GameObject bolimPrefab;
    public GameObject bochaTimeAPrefab; // bocha do jogador
    public GameObject bochaTimeBPrefab; // bocha do adversário (CPU)
    [Tooltip("Sorting order do bolim. Maior que o das bochas pra ele nunca ficar escondido embaixo de uma bola.")]
    public int ordemBolim = 10;

    [Header("Tempo da rodada")]
    // tempo = tempoExtra + tempoPorBocha * bochasPorTime, e encolhe com as fases até 'fracaoMinima' do valor base
    public float tempoExtra = 4f;
    public float tempoPorBocha = 7f;
    public float reducaoPorFase = 0.3f;
    [Range(0.3f, 1f)] public float fracaoMinima = 0.75f;

    [Header("Espera das bolas")]
    public float velocidadeParada = 0.3f; // abaixo disso a bola é considerada parada
    public float tempoMaxEspera = 5f;     // segurança: nunca trava o turno esperando bola

    [Header("IA do Adversário (Time B)")]
    public float erroAnguloCPU = 12f;   // graus de imprecisão do lance da CPU (menor = mais difícil)
    public float atrasoLanceCPU = 0.5f; // pequena pausa antes do lance do adversário, pra não parecer instantâneo

    [Header("UI")]
    public TMP_Text turnText;   // opcional: o BochinhaHUD já mostra o turno e o resultado
    public TMP_Text scoreText;  // opcional
    public BochinhaHUD hud;     // se vazio, é criado automaticamente

    [HideInInspector] public GameObject currentBolim;

    private int bochasJogador = 0;
    private int bochasCPU = 0;
    private EstadoRodada estadoAtual = EstadoRodada.AguardandoBolim;

    private List<GameObject> todasBochas = new List<GameObject>();

    private float tempoDaRodada = 15f;
    private float elapsed = 0f;
    private bool rodadaEncerrada = false; // só usado no modo teste (cena rodando sem o GameManagerRework)

    /// <summary>0 a 1: quanto do tempo da rodada já passou (usado pelo HUD).</summary>
    public float TempoNormalizado => Mathf.Clamp01(elapsed / Mathf.Max(0.01f, tempoDaRodada));
    public bool AguardandoBolim => estadoAtual == EstadoRodada.AguardandoBolim;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        tempoDaRodada = TempoBase(0);

        if (hud == null) hud = FindObjectOfType<BochinhaHUD>();
        if (hud == null) hud = gameObject.AddComponent<BochinhaHUD>();
    }

    void Start()
    {
        StartCoroutine(IniciarRodada());
    }

    void Update()
    {
        if (jogoFinalizado || rodadaEncerrada) return;

        elapsed += Time.deltaTime;

        // Modo teste: sem GameManagerRework ninguém chama TempoEsgotado, então chamamos nós mesmos
        if (elapsed >= tempoDaRodada && GameManagerRework.Instance == null) TempoEsgotado();
    }

    // --- INTEGRAÇÃO COM O NOVO CORE ---
    private float TempoBase(int faseAtual)
    {
        float baseTotal = tempoExtra + tempoPorBocha * bochasPorTime;
        return Mathf.Max(baseTotal * fracaoMinima, baseTotal - faseAtual * reducaoPorFase);
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // A Bocha exige assentamento físico da bola, então o tempo depende de quantas bochas cada time lança
        // e só escala levemente conforme a fase avança (o valor retornado vira o timer do GameManager).
        tempoDaRodada = TempoBase(faseAtual);

        // Conforme a fase avança, o adversário mira melhor -> fica mais difícil vencer.
        erroAnguloCPU = Mathf.Max(4f, 12f - (faseAtual * 0.5f));

        return tempoDaRodada;
    }

    IEnumerator IniciarRodada()
    {
        estadoAtual = EstadoRodada.AguardandoBolim;
        MostrarTurno(TurnoHUD.Bolim, "Lance o Bolim!", Color.white);
        AtualizarContagemHUD();
        BochinhaLauncher.Instance.SetupTurn(bolimPrefab, true);
        yield return null;
    }

    // Chamado pelo Launcher sempre que O JOGADOR solta uma bola (bolim ou bocha)
    public void BolaLancada(GameObject bolaGerada)
    {
        if (jogoFinalizado || rodadaEncerrada) return; // Trava de segurança
        MostrarTurno(TurnoHUD.Aguardando, "Aguarde...", Color.white);
        RegistrarBolaEAguardar(bolaGerada, ehJogador: true);
    }

    // Lance automático do adversário (não passa pelo Launcher, não depende de input)
    private void LancarBochaCPU()
    {
        if (jogoFinalizado || rodadaEncerrada) return;
        StartCoroutine(RotinaLanceCPU());
    }

    private IEnumerator RotinaLanceCPU()
    {
        yield return new WaitForSeconds(atrasoLanceCPU);
        if (jogoFinalizado || rodadaEncerrada) yield break;

        GameObject novaBola = Instantiate(bochaTimeBPrefab, pontoDeLancamento.position, Quaternion.identity);
        Rigidbody2D rb2d = novaBola.GetComponent<Rigidbody2D>();

        if (rb2d != null && currentBolim != null)
        {
            // Mesma física da bola do jogador (sem gravidade, arrasto, quique)
            BochinhaLauncher.Instance.PrepararBola(rb2d);

            Vector2 origem = pontoDeLancamento.position;
            Vector2 alvo = currentBolim.transform.position;
            Vector2 direcaoBase = (alvo - origem).normalized;

            // Imprecisão do adversário, pra ficar justo com o jogador
            float anguloErro = Random.Range(-erroAnguloCPU, erroAnguloCPU);
            Vector2 direcaoFinal = Quaternion.Euler(0f, 0f, anguloErro) * direcaoBase;

            // Força calibrada com o arrasto: a bola para perto do bolim em vez de passar direto
            float distanciaAlvo = Vector2.Distance(origem, alvo);
            float forcaBase = BochinhaLauncher.Instance.ForcaParaAlcance(distanciaAlvo, rb2d.mass);
            float forcaFinal = Mathf.Min(forcaBase * Random.Range(0.85f, 1.15f), BochinhaLauncher.Instance.maxForce);

            rb2d.AddForce(direcaoFinal * forcaFinal, ForceMode2D.Impulse);
        }

        RegistrarBolaEAguardar(novaBola, ehJogador: false);
    }

    private void RegistrarBolaEAguardar(GameObject bola, bool ehJogador)
    {
        if (estadoAtual == EstadoRodada.AguardandoBolim)
        {
            currentBolim = bola;

            // O bolim é pequeno: sempre desenhado por cima das bochas, senão some embaixo delas
            SpriteRenderer sr = bola.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = ordemBolim;
        }
        else if (ehJogador)
        {
            // Renomeia explicitamente pra o ScoreManager identificar o time certo,
            // independente de como o prefab foi nomeado no projeto.
            bola.name = "BochaTimeA";
            todasBochas.Add(bola);
            bochasJogador++;
        }
        else
        {
            bola.name = "BochaTimeB";
            todasBochas.Add(bola);
            bochasCPU++;
        }

        AtualizarContagemHUD();
        StartCoroutine(AguardarTudoParar());
    }

    // Espera TODAS as bolas pararem (uma bola nova pode empurrar as que já estavam na quadra)
    IEnumerator AguardarTudoParar()
    {
        yield return new WaitForSeconds(0.4f);

        float esperado = 0f;
        while (esperado < tempoMaxEspera && ExisteBolaEmMovimento())
        {
            yield return new WaitForSeconds(0.1f);
            esperado += 0.1f;
        }

        ZerarVelocidades();
        AvancarFluxo();
    }

    private bool ExisteBolaEmMovimento()
    {
        float limite = velocidadeParada * velocidadeParada;

        if (BolaMovendo(currentBolim, limite)) return true;
        foreach (GameObject b in todasBochas)
            if (BolaMovendo(b, limite)) return true;

        return false;
    }

    private bool BolaMovendo(GameObject bola, float limiteQuadrado)
    {
        if (bola == null) return false;
        Rigidbody2D rb = bola.GetComponent<Rigidbody2D>();
        return rb != null && rb.velocity.sqrMagnitude > limiteQuadrado;
    }

    private void ZerarVelocidades()
    {
        ZerarBola(currentBolim);
        foreach (GameObject b in todasBochas) ZerarBola(b);
    }

    private void ZerarBola(GameObject bola)
    {
        if (bola == null) return;
        Rigidbody2D rb = bola.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void AvancarFluxo()
    {
        if (jogoFinalizado || rodadaEncerrada) return;

        switch (estadoAtual)
        {
            case EstadoRodada.AguardandoBolim:
                estadoAtual = EstadoRodada.TurnoJogador;
                PrepararLancamentoJogador();
                break;

            case EstadoRodada.TurnoJogador:
                if (bochasCPU < bochasPorTime)
                {
                    estadoAtual = EstadoRodada.TurnoCPU;
                    MostrarTurno(TurnoHUD.Adversario, "Vez do adversário...", Color.red);
                    LancarBochaCPU();
                }
                else
                {
                    CalcularPontuacaoFinal();
                }
                break;

            case EstadoRodada.TurnoCPU:
                if (bochasJogador < bochasPorTime)
                {
                    estadoAtual = EstadoRodada.TurnoJogador;
                    PrepararLancamentoJogador();
                }
                else
                {
                    CalcularPontuacaoFinal();
                }
                break;
        }
    }

    void PrepararLancamentoJogador()
    {
        int restantes = bochasPorTime - bochasJogador;
        MostrarTurno(TurnoHUD.Jogador, $"Sua vez! Bochas restantes: {restantes}", Color.blue);

        BochinhaLauncher.Instance.SetupTurn(bochaTimeAPrefab, false);
    }

    void CalcularPontuacaoFinal()
    {
        estadoAtual = EstadoRodada.Finalizado;
        MostrarTurno(TurnoHUD.Fim, "Fim da rodada!", Color.white);

        if (BochinhaScoreManager.Instance == null || currentBolim == null)
        {
            Debug.LogWarning("[Bochinha] Sem ScoreManager ou sem bolim na quadra: contando como derrota.");
            FinalizarPartida("Time B");
            return;
        }

        BochinhaScoreManager.Instance.EvaluateRound(todasBochas, currentBolim.transform);
    }

    // --- MÉTODOS DE RESOLUÇÃO DO MINIGAME ---
    public void FinalizarPartida(string equipeVencedora)
    {
        if (jogoFinalizado || rodadaEncerrada) return;

        // O jogador sempre controla o Time A
        bool ganhou = equipeVencedora == "Time A";

        if (hud != null)
            hud.MostrarResultado(ganhou ? "VOCE VENCEU!" : "O ADVERSARIO VENCEU...", ganhou ? hud.corVitoria : hud.corDerrota, false);

        if (ganhou)
        {
            if (GameManagerRework.Instance != null) Vencer();
            else EncerrarModoTeste("VITÓRIA");
        }
        else
        {
            if (GameManagerRework.Instance != null) Perder();
            else EncerrarModoTeste("DERROTA");
        }
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado || rodadaEncerrada) return;

        if (BochinhaLauncher.Instance != null) BochinhaLauncher.Instance.DesativarLancamento();
        if (hud != null) hud.MostrarResultado("TEMPO ESGOTADO!", hud.corDerrota, true);

        if (GameManagerRework.Instance != null) base.TempoEsgotado();
        else EncerrarModoTeste("DERROTA (tempo esgotado)");
    }

    // Sem GameManagerRework (cena rodando sozinha), Vencer()/Perder() dão NullReferenceException
    private void EncerrarModoTeste(string resultado)
    {
        rodadaEncerrada = true;
        Debug.Log("[Bochinha] " + resultado + " (modo teste: sem GameManagerRework na cena. Pare e dê Play de novo, ou comece pela cena Menu.)");
    }

    // --- Helpers de UI ---
    private void MostrarTurno(TurnoHUD turno, string textoAntigo, Color corAntiga)
    {
        if (turnText != null) { turnText.text = textoAntigo; turnText.color = corAntiga; }
        if (hud != null) hud.MostrarTurno(turno);
    }

    private void AtualizarContagemHUD()
    {
        if (hud != null)
            hud.AtualizarContagem(bochasPorTime - bochasJogador, bochasPorTime - bochasCPU, bochasPorTime);
    }
}