using UnityEngine;

public class GameManagerCapivara : MinigameBase
{
    public static GameManagerCapivara Instance;

    [Header("Configuração")]
    [Tooltip("Deixe em 0 para contar automaticamente as capivaras presentes na cena no início da fase.")]
    public int totalCapivaras;
    private int capivarasEncontradas = 0;

    [Header("Telas / Painéis")]
    public GameObject painelVitoria;
    public GameObject painelDerrota;
    public GameObject telaVermelha;

    // Mantemos essa propriedade pública para que o CameraPanMobile.cs continue funcionando
    public bool JogoEncerrado => jogoFinalizado;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // --- INTEGRAÇÃO COM O NOVO CORE ---
    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // O tempo já diminui sozinho na base do GameManagerRework
        return tempoGlobalSugerido;
    }

    void Start()
    {
        if (totalCapivaras <= 0)
        {
            ClickableObject[] objetos = FindObjectsOfType<ClickableObject>();
            int contagem = 0;
            foreach (var obj in objetos)
            {
                if (obj.isCapivara) contagem++;
            }
            totalCapivaras = contagem;
        }
    }

    public void CapivaraEncontrada()
    {
        if (jogoFinalizado) return; 

        capivarasEncontradas++;
        Debug.Log("Capivara encontrada! Total: " + capivarasEncontradas + "/" + totalCapivaras);

        if (capivarasEncontradas >= totalCapivaras)
        {
            FaseConcluida();
        }
    }

    public void GameOver()
    {
        if (jogoFinalizado) return; 

        if (telaVermelha != null) telaVermelha.SetActive(true);
        if (painelDerrota != null) painelDerrota.SetActive(true);

        Perder(); // Avisa o núcleo global que o jogador clicou errado
    }

    private void FaseConcluida()
    {
        if (jogoFinalizado) return;

        Debug.Log("Venceu a fase!");
        if (painelVitoria != null) painelVitoria.SetActive(true);

        Vencer(); // Avisa o núcleo global que o jogador encontrou todas
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;
        
        if (telaVermelha != null) telaVermelha.SetActive(true);
        if (painelDerrota != null) painelDerrota.SetActive(true);

        base.TempoEsgotado();
    }
}