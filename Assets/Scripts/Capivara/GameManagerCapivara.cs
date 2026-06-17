using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerCapivara : MonoBehaviour
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

    // CORRIGIDO: sem essa flag, o jogador conseguia continuar clicando
    // (e até vencer ou perder de novo) depois que a fase já tinha terminado.
    private bool jogoEncerrado = false;
    public bool JogoEncerrado => jogoEncerrado;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        // CORRIGIDO: se o total não foi definido manualmente no Inspector,
        // conta automaticamente quantas capivaras existem na fase.
        // Evita o jogo nunca terminar (ou terminar errado) por esquecimento.
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
        if (jogoEncerrado) return; // CORRIGIDO: ignora se a fase já acabou

        capivarasEncontradas++;
        Debug.Log("Capivara encontrada! Total: " + capivarasEncontradas + "/" + totalCapivaras);

        if (capivarasEncontradas >= totalCapivaras)
        {
            FaseConcluida();
        }
    }

    public void GameOver()
    {
        if (jogoEncerrado) return; // CORRIGIDO: ignora se já tinha vencido/perdido antes

        jogoEncerrado = true;

        if (telaVermelha != null)
        {
            telaVermelha.SetActive(true);
        }

        if (painelDerrota != null)
        {
            painelDerrota.SetActive(true);
        }

        // CORRIGIDO: congela o jogo de fato ao perder, em vez de só mostrar o painel
        Time.timeScale = 0f;
    }

    private void FaseConcluida()
    {
        if (jogoEncerrado) return;

        jogoEncerrado = true;

        Debug.Log("Venceu a fase!");
        if (painelVitoria != null) painelVitoria.SetActive(true);

        // CORRIGIDO: congela o jogo também na vitória
        Time.timeScale = 0f;
    }

    public void ReiniciarFase()
    {
        // CORRIGIDO: se não voltar o timeScale para 1, a cena recarregada
        // nasceria pausada e nada se moveria.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
