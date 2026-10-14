using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManagerCapivara : MinigameBase
{
    public static GameManagerCapivara Instance;

    [Header("Configuração")]
    [Tooltip("Deixe em 0 para contar automaticamente as capivaras presentes na cena no início da fase.")]
    public int totalCapivaras = 5;
    private int capivarasEncontradas = 0;

    [Header("Telas / Painéis")]
    public GameObject painelVitoria;
    public GameObject painelDerrota;
    public GameObject telaVermelha;
    public GameObject[] spawnpoints;
    public Sprite[] animais;
    List<int> numerosAleatorios = new List<int>();
    public TMP_Text progresso;
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
        progresso.text = $"{capivarasEncontradas} / {totalCapivaras}";
        for (int i = 0; i < totalCapivaras; i++)
        {
            int random = 0;
            while (numerosAleatorios.Contains(random))
                random = Random.Range(0, spawnpoints.Length);
            numerosAleatorios.Add(random);
            spawnpoints[random].GetComponent<SpriteRenderer>().sprite = animais[0];
            spawnpoints[random].GetComponent<ClickableObject>().isCapivara = true;
        }

        for (int i = 0; i < spawnpoints.Length; i++)
        {
            if (numerosAleatorios.Contains(i))
                continue;
            spawnpoints[i].GetComponent<SpriteRenderer>().sprite = Random.value > 0.5f ? animais[1] : animais[2];
            spawnpoints[i].GetComponent<ClickableObject>().isCapivara = false;
        }
    }

    public void CapivaraEncontrada()
    {
        if (jogoFinalizado) return; 

        capivarasEncontradas++;
        progresso.text = $"{capivarasEncontradas} / {totalCapivaras}";

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