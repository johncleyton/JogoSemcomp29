using UnityEngine;
using UnityEngine.UI;

public enum EstadoOvelha
{
    MuitoNova,
    EmPerfeitoEstado,
    MuitoVelha
}

public enum EscolhaJogador
{
    PenteMenor,  // Para Tosa Normal (Ovelha em Perfeito Estado)
    PenteMaior,  // Para Tosa Parcial (Ovelha Muito Velha)
    Descarta     // Para Não Tosar (Ovelha Muito Nova)
}

public class TosquieManager : MonoBehaviour
{
    public static TosquieManager Instance;

    [Header("Referências")]
    public Transform centroTosaPoint;  // Onde a ovelha atual fica
    public GameObject ovelhaPrefab;    // Prefab do Sprite/Objeto da ovelha

    [Header("UI")]
    public Text estadoOvelhaText;      // Texto que indica o estado/dica da ovelha
    public Text placarText;            // Texto de pontuação (ex: 3/6)
    public Text resultadoText;         // Texto de Vitória/Derrota
    public Slider timerSlider;         // Barra de tempo da rodada

    [Header("Configs")]
    public int totalParaVencer = 6;
    public float tempoPorOvelha = 4f;

    [Header("Estado Atual (Somente Leitura)")]
    public EstadoOvelha estadoOvelhaAtual;
    private int acertos = 0;
    private float tempoRestante;
    private bool isGameOver = false;

    private GameObject ovelhaInstanciada;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        acertos = 0;
        isGameOver = false;
        if (resultadoText != null) resultadoText.text = "";
        
        GerarNovaOvelha();
    }

    void Update()
    {
        if (isGameOver) return;

        tempoRestante -= Time.deltaTime;

        if (timerSlider != null)
        {
            timerSlider.value = tempoRestante / tempoPorOvelha;
        }

        if (tempoRestante <= 0)
        {
            FinalizarJogo(false, "Tempo Esgotado!");
        }
    }

    private void GerarNovaOvelha()
    {
        if (ovelhaInstanciada != null)
        {
            Destroy(ovelhaInstanciada);
        }

        // Sorteia um estado para a ovelha
        estadoOvelhaAtual = (EstadoOvelha)Random.Range(0, 3);

        // Instancia a ovelha no centro
        if (ovelhaPrefab != null && centroTosaPoint != null)
        {
            ovelhaInstanciada = Instantiate(ovelhaPrefab, centroTosaPoint.position, Quaternion.identity);
        }

        tempoRestante = tempoPorOvelha;
        AtualizarUI();
    }

    private void AtualizarUI()
    {
        if (placarText != null)
        {
            placarText.text = $"{acertos}/{totalParaVencer}";
        }

        if (estadoOvelhaText != null)
        {
            switch (estadoOvelhaAtual)
            {
                case EstadoOvelha.MuitoNova:
                    estadoOvelhaText.text = "Ovelha: MUITO NOVA";
                    break;
                case EstadoOvelha.EmPerfeitoEstado:
                    estadoOvelhaText.text = "Ovelha: PERFEITA";
                    break;
                case EstadoOvelha.MuitoVelha:
                    estadoOvelhaText.text = "Ovelha: MUITO VELHA";
                    break;
            }
        }
    }

    // Método chamado pelos Botões da UI (0 = Pente Menor, 1 = Pente Maior, 2 = Descarta)
    public void ReceberEscolha(int escolhaIndex)
    {
        if (isGameOver) return;

        EscolhaJogador escolha = (EscolhaJogador)escolhaIndex;
        bool acertou = false;

        // Validação das regras de negócio:
        // 1. Perfeita -> Pente Menor (Tosa Normal)
        // 2. Muito Velha -> Pente Maior (Tosa Parcial)
        // 3. Muito Nova -> Descarta / Não Tosa
        switch (estadoOvelhaAtual)
        {
            case EstadoOvelha.EmPerfeitoEstado:
                acertou = (escolha == EscolhaJogador.PenteMenor);
                break;

            case EstadoOvelha.MuitoVelha:
                acertou = (escolha == EscolhaJogador.PenteMaior);
                break;

            case EstadoOvelha.MuitoNova:
                acertou = (escolha == EscolhaJogador.Descarta);
                break;
        }

        if (acertou)
        {
            acertos++;
            if (acertos >= totalParaVencer)
            {
                FinalizarJogo(true, "Mestre da Tosa!");
            }
            else
            {
                GerarNovaOvelha();
            }
        }
        else
        {
            FinalizarJogo(false, "Pente Errado!");
        }
    }

    private void FinalizarJogo(bool vitoria, string mensagem)
    {
        isGameOver = true;
        
        if (resultadoText != null)
        {
            resultadoText.text = vitoria ? $"VITÓRIA!\n{mensagem}" : $"DERROTA!\n{mensagem}";
            resultadoText.color = vitoria ? Color.green : Color.red;
        }

        Debug.Log(vitoria ? $"[VITÓRIA] {mensagem}" : $"[DERROTA] {mensagem}");
    }
}