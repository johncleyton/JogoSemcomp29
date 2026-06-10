using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameManagerCapivara : MonoBehaviour
{
    public static GameManagerCapivara Instance;

    public int totalCapivaras;
    private int capivarasEncontradas = 0;

    public GameObject painelVitoria;
    public GameObject painelDerrota;
    public GameObject telaVermelha; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void CapivaraEncontrada()
    {
        capivarasEncontradas++;
        Debug.Log("Capivara encontrada! Total: " + capivarasEncontradas + "/" + totalCapivaras);

        if (capivarasEncontradas >= totalCapivaras)
        {
            FaseConcluida();
        }
    }

    public void GameOver()
    {
        // CORRIGIDO: Agora está chamando "telaVermelha" exatamente como foi declarado em cima
        if (telaVermelha != null) 
        {
            telaVermelha.SetActive(true);
        }

        // Ativa o painel de derrota original (com botões de reiniciar, etc)
        if (painelDerrota != null)
        {
            painelDerrota.SetActive(true);
        }
    }

    private void FaseConcluida()
    {
        Debug.Log("Venceu a fase!");
        if (painelVitoria != null) painelVitoria.SetActive(true);
    }

    public void ReiniciarFase()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}