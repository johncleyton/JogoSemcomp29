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
    public int bochasPorTime = 2; // reduzido pra caber no tempo fixo do minigame (era 4, pensado pra 2 jogadores)
    public Transform pontoDeLancamento;
    public GameObject bolimPrefab;
    public GameObject bochaTimeAPrefab; // bocha do jogador
    public GameObject bochaTimeBPrefab; // bocha do adversário (CPU)

    [Header("IA do Adversário (Time B)")]
    public float erroAnguloCPU = 12f;   // graus de imprecisão do lance da CPU (menor = mais difícil)
    public float atrasoLanceCPU = 0.5f; // pequena pausa antes do lance do adversário, pra não parecer instantâneo

    [Header("UI")]
    public TMP_Text turnText;
    public TMP_Text scoreText;

    [HideInInspector] public GameObject currentBolim;

    private int bochasJogador = 0;
    private int bochasCPU = 0;
    private EstadoRodada estadoAtual = EstadoRodada.AguardandoBolim;

    private List<GameObject> todasBochas = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(IniciarRodada());
    }

    // --- INTEGRAÇÃO COM O NOVO CORE ---
    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // A Bocha exige assentamento físico da bola, então usamos um tempo fixo maior,
        // mas ainda escalamos levemente conforme a fase avança.
        float tempoFixo = Mathf.Max(10f, 15f - (faseAtual * 0.3f));

        // Conforme a fase avança, o adversário mira melhor -> fica mais difícil vencer.
        erroAnguloCPU = Mathf.Max(4f, 12f - (faseAtual * 0.5f));

        return tempoFixo;
    }

    IEnumerator IniciarRodada()
    {
        turnText.text = "Lance o Bolim!";
        estadoAtual = EstadoRodada.AguardandoBolim;
        BochinhaLauncher.Instance.SetupTurn(bolimPrefab, true);
        yield return null;
    }

    // Chamado pelo Launcher sempre que O JOGADOR solta uma bola (bolim ou bocha)
    public void BolaLancada(GameObject bolaGerada)
    {
        if (jogoFinalizado) return; // Trava de segurança
        RegistrarBolaEAguardar(bolaGerada, ehJogador: true);
    }

    // Lance automático do adversário (não passa pelo Launcher, não depende de input)
    private void LancarBochaCPU()
    {
        if (jogoFinalizado) return;
        StartCoroutine(RotinaLanceCPU());
    }

    private IEnumerator RotinaLanceCPU()
    {
        yield return new WaitForSeconds(atrasoLanceCPU);
        if (jogoFinalizado) yield break;

        GameObject novaBola = Instantiate(bochaTimeBPrefab, pontoDeLancamento.position, Quaternion.identity);
        Rigidbody2D rb2d = novaBola.GetComponent<Rigidbody2D>();

        if (rb2d != null && currentBolim != null)
        {
            Vector2 origem = pontoDeLancamento.position;
            Vector2 alvo = currentBolim.transform.position;
            Vector2 direcaoBase = (alvo - origem).normalized;

            // Imprecisão do adversário, pra ficar justo com o jogador
            float anguloErro = Random.Range(-erroAnguloCPU, erroAnguloCPU);
            Vector2 direcaoFinal = Quaternion.Euler(0f, 0f, anguloErro) * direcaoBase;

            float distanciaAlvo = Vector2.Distance(origem, alvo);
            float forcaBase = Mathf.Clamp(distanciaAlvo * 0.8f, 2f, BochinhaLauncher.Instance.maxForce);
            float forcaFinal = forcaBase * Random.Range(0.85f, 1.15f);

            rb2d.AddForce(direcaoFinal * forcaFinal, ForceMode2D.Impulse);
        }

        RegistrarBolaEAguardar(novaBola, ehJogador: false);
    }

    private void RegistrarBolaEAguardar(GameObject bola, bool ehJogador)
    {
        if (estadoAtual == EstadoRodada.AguardandoBolim)
        {
            currentBolim = bola;
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

        StartCoroutine(AguardarBolaParar(bola.GetComponent<Rigidbody2D>()));
    }

    IEnumerator AguardarBolaParar(Rigidbody2D rb2d)
    {
        yield return new WaitForSeconds(0.5f);

        while (rb2d != null && rb2d.velocity.magnitude > 0.05f)
        {
            yield return new WaitForSeconds(0.2f);
        }

        if (rb2d != null)
        {
            rb2d.velocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }

        AvancarFluxo();
    }

    void AvancarFluxo()
    {
        if (jogoFinalizado) return;

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
                    turnText.text = "Vez do adversário...";
                    turnText.color = Color.red;
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
        turnText.text = $"Sua vez! Bochas restantes: {restantes}";
        turnText.color = Color.blue;

        BochinhaLauncher.Instance.SetupTurn(bochaTimeAPrefab, false);
    }

    void CalcularPontuacaoFinal()
    {
        estadoAtual = EstadoRodada.Finalizado;
        turnText.text = "Fim da rodada!";
        BochinhaScoreManager.Instance.EvaluateRound(todasBochas, currentBolim.transform);
    }

    // --- MÉTODOS DE RESOLUÇÃO DO MINIGAME ---
    public void FinalizarPartida(string equipeVencedora)
    {
        if (jogoFinalizado) return;

        // O jogador sempre controla o Time A
        if (equipeVencedora == "Time A")
        {
            Vencer();
        }
        else
        {
            Perder();
        }
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;
        base.TempoEsgotado();
    }
}