using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;

public class TosquieManager : MinigameBase
{
    public static TosquieManager Instance;

    [Header("Fila de ovelhas")]
    [Tooltip("Prefab da Ovelha (Assets/Prefabs/TosquieAOvelha/Ovelha).")]
    public GameObject ovelhaPrefab;
    [Tooltip("Onde cada ovelha nova aparece. Pode usar o objeto 'Centro'.")]
    public Transform pontoSpawnOvelha;

    [Header("Tosquiadora")]
    public TosquiadoraController tosquiadora;

    [Header("UI - Placar e status")]
    public TMP_Text placarText;
    public TMP_Text tentativasText;

    [Header("UI - Botões dos pentes")]
    public Button botaoPenteMenor;   // Ovelha EmPerfeitoEstado -> Pente Menor (Index 0)
    public Button botaoPenteMaior;   // Ovelha MuitoVelha        -> Pente Maior (Index 1)
    public Button botaoDescarta;     // Ovelha MuitoNova         -> Descarta   (Index 2)

    [Header("Configurações")]
    [FormerlySerializedAs("totalFatias")]
    [Tooltip("Quantas ovelhas precisam ser tosquiadas corretamente para vencer o minigame.")]
    public int ovelhasNecessarias = 6;
    [Tooltip("Quantas vezes a tosquiadora pode cair sem acertar lã, por ovelha, antes dela escapar.")]
    public int tentativasPorOvelha = 3;

    private OvelhaController ovelhaAtual;
    private int pedacosRestantesNaOvelha;
    private int ovelhasConcluidas = 0;
    private int tentativasRestantes;
    private bool aguardandoEscolha = true;

    public bool JogoEncerrado => jogoFinalizado;
    public bool FaseDeTosquia => !aguardandoEscolha;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        return tempoGlobalSugerido;
    }

    void Start()
    {
        ovelhasConcluidas = 0;
        AtualizarPlacar();

        if (botaoPenteMenor != null) botaoPenteMenor.onClick.AddListener(EscolherPenteMenor);
        if (botaoPenteMaior != null) botaoPenteMaior.onClick.AddListener(EscolherPenteMaior);
        if (botaoDescarta != null) botaoDescarta.onClick.AddListener(EscolherDescarta);

        if (tosquiadora != null) tosquiadora.PodeAgir = false;

        NovaOvelha();
    }

    void NovaOvelha()
    {
        if (ovelhaPrefab == null)
        {
            Debug.LogError("TosquieManager: 'Ovelha Prefab' não foi atribuído no Inspector.");
            return;
        }

        Vector3 posicao = pontoSpawnOvelha != null ? pontoSpawnOvelha.position : Vector3.zero;
        GameObject instancia = Instantiate(ovelhaPrefab, posicao, Quaternion.identity);
        ovelhaAtual = instancia.GetComponent<OvelhaController>();

        if (ovelhaAtual == null)
        {
            Debug.LogError("TosquieManager: o prefab da Ovelha não tem o componente OvelhaController.");
            return;
        }

        EstadoOvelha novoEstado = (EstadoOvelha)Random.Range(0, 3);
        ovelhaAtual.Inicializar(novoEstado);

        pedacosRestantesNaOvelha = ContarPedacosDeLa(instancia);
        if (pedacosRestantesNaOvelha <= 0)
        {
            Debug.LogWarning("TosquieManager: a ovelha instanciada não tem nenhum filho com a tag 'La");
        }

        aguardandoEscolha = true;
        tentativasRestantes = tentativasPorOvelha;
        AtualizarTentativas();
        DefinirBotoesInterativos(true);

        if (tosquiadora != null) tosquiadora.PodeAgir = false;
    }

    int ContarPedacosDeLa(GameObject raiz)
    {
        int total = 0;
        foreach (Transform filho in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (filho.CompareTag("La")) total++;
        }
        return total;
    }

    public void EscolherPenteMenor() => ProcessarEscolha(EstadoOvelha.EmPerfeitoEstado);
    public void EscolherPenteMaior() => ProcessarEscolha(EstadoOvelha.MuitoVelha);
    public void EscolherDescarta() => ProcessarEscolha(EstadoOvelha.MuitoNova);

    void ProcessarEscolha(EstadoOvelha estadoEscolhido)
    {
        if (jogoFinalizado || !aguardandoEscolha || ovelhaAtual == null) return;

        if (estadoEscolhido == ovelhaAtual.estadoAtual)
        {
            IniciarTosquia();
        }
        else
        {
            StartCoroutine(FeedbackErro());
        }
    }

    IEnumerator FeedbackErro()
    {
        DefinirBotoesInterativos(false);
        yield return new WaitForSeconds(0.35f);
        if (!jogoFinalizado && aguardandoEscolha)
            DefinirBotoesInterativos(true);
    }

    void IniciarTosquia()
    {
        aguardandoEscolha = false;
        DefinirBotoesInterativos(false);

        if (tosquiadora != null)
        {
            tosquiadora.ResetarParaTopo();
            tosquiadora.PodeAgir = true;
        }
    }

    void DefinirBotoesInterativos(bool valor)
    {
        if (botaoPenteMenor != null) botaoPenteMenor.interactable = valor;
        if (botaoPenteMaior != null) botaoPenteMaior.interactable = valor;
        if (botaoDescarta != null) botaoDescarta.interactable = valor;
    }

    // Chamado pela TosquiadoraController quando ela acerta um pedaço de lã (tag "La")
    public void CortouLa()
    {
        if (jogoFinalizado) return;

        pedacosRestantesNaOvelha--;

        if (pedacosRestantesNaOvelha > 0)
        {
            // ainda sobra lã nesta ovelha: a tosquiadora continua ativa para o próximo pedaço
            return;
        }

        // ovelha totalmente tosquiada
        ovelhasConcluidas++;
        AtualizarPlacar();

        if (ovelhasConcluidas >= ovelhasNecessarias)
        {
            if (tosquiadora != null) tosquiadora.PodeAgir = false;
            Vencer();
            return;
        }

        EncerrarOvelhaAtual();
    }

    // Chamado pela TosquiadoraController quando ela cai sem acertar lã
    public void TentativaFalhou()
    {
        if (jogoFinalizado) return;

        tentativasRestantes--;
        AtualizarTentativas();

        if (tentativasRestantes <= 0)
        {
            // ovelha escapou sem ser totalmente tosquiada
            EncerrarOvelhaAtual();
        }
    }

    void EncerrarOvelhaAtual()
    {
        if (tosquiadora != null) tosquiadora.PodeAgir = false;
        if (ovelhaAtual != null)
        {
            ovelhaAtual.SairDaTela();
            ovelhaAtual = null;
        }
        NovaOvelha();
    }

    void AtualizarPlacar()
    {
        if (placarText != null)
            placarText.text = $"{ovelhasConcluidas}/{ovelhasNecessarias}";
    }

    void AtualizarTentativas()
    {
        if (tentativasText != null)
            tentativasText.text = $"Tentativas: {tentativasRestantes}";
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;
        Perder();
        base.TempoEsgotado();
    }
}