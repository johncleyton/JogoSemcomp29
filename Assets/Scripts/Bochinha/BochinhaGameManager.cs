using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; // textmeshpro
public class BochinhaGameManager : MinigameBase
{
    public static BochinhaGameManager Instance;

    [Header("Configurações do Jogo")]
    public int bochasPorTime = 4;
    public Transform pontoDeLancamento;
    public GameObject bolimPrefab;
    public GameObject bochaTimeAPrefab; 
    public GameObject bochaTimeBPrefab;

    [Header("UI")]
    public TMP_Text turnText;
    public TMP_Text scoreText;

    [HideInInspector] public GameObject currentBolim;
    
    private int bochasTimeA = 0;
    private int bochasTimeB = 0;
    private bool isTeamATurn = true;
    private bool isBolimEmJogo = false;
    private bool esperandoBolaParar = false;

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
        // A Bocha exige cálculo físico de parada, necessitando de um tempo fixo maior (ex: 15s)
        return 15f; 
    }

    IEnumerator IniciarRodada()
    {
        turnText.text = "Lance o Bolim!";
        isBolimEmJogo = false;
        BochinhaLauncher.Instance.SetupTurn(bolimPrefab, true);
        yield return null;
    }

    public void BolaLancada(GameObject bolaGerada)
    {
        if (jogoFinalizado) return; // Trava de segurança

        esperandoBolaParar = true;
        
        if (!isBolimEmJogo)
        {
            currentBolim = bolaGerada;
        }
        else
        {
            todasBochas.Add(bolaGerada);
            if (isTeamATurn) bochasTimeA++;
            else bochasTimeB++;
        }

        StartCoroutine(AguardarBolaParar(bolaGerada.GetComponent<Rigidbody2D>()));
    }

    IEnumerator AguardarBolaParar(Rigidbody2D rb2d)
    {
        yield return new WaitForSeconds(0.5f);

        while (rb2d.velocity.magnitude > 0.05f)
        {
            yield return new WaitForSeconds(0.2f);
        }

        rb2d.velocity = Vector2.zero;
        rb2d.angularVelocity = 0f;
        esperandoBolaParar = false;

        AvancarTurno();
    }

    void AvancarTurno()
    {
        if (jogoFinalizado) return; // Trava de segurança

        if (!isBolimEmJogo)
        {
            isBolimEmJogo = true;
            isTeamATurn = true;
            PrepararLancamentoBocha();
            return;
        }

        if (bochasTimeA >= bochasPorTime && bochasTimeB >= bochasPorTime)
        {
            CalcularPontuacaoFinal();
            return;
        }

        isTeamATurn = !isTeamATurn;

        if (isTeamATurn && bochasTimeA >= bochasPorTime) isTeamATurn = false;
        else if (!isTeamATurn && bochasTimeB >= bochasPorTime) isTeamATurn = true;

        PrepararLancamentoBocha();
    }

    void PrepararLancamentoBocha()
    {
        turnText.text = isTeamATurn ? "Turno: Time A" : "Turno: Time B";
        turnText.color = isTeamATurn ? Color.blue : Color.red;

        GameObject prefabDaVez = isTeamATurn ? bochaTimeAPrefab : bochaTimeBPrefab;
        BochinhaLauncher.Instance.SetupTurn(prefabDaVez, false);
    }

    void CalcularPontuacaoFinal()
    {
        turnText.text = "Fim da Rodada!";
        BochinhaScoreManager.Instance.EvaluateRound(todasBochas, currentBolim.transform);
    }

    // --- MÉTODOS DE RESOLUÇÃO DO MINIGAME ---
    public void FinalizarPartida(string equipeVencedora)
    {
        if (jogoFinalizado) return;

        // Assumindo que o jogador controla o Time A
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