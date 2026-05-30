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
    // Start is called before the first frame update

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void CapivaraEncontrada()
    {
        capivarasEncontradas++;
        Debug.Log("Capivara encontrada! Total: " + capivarasEncontradas + "/" + totalCapivaras);

        if(capivarasEncontradas >= totalCapivaras)
        {
            FaseConcluida();
        }
    }

    public void GameOver()
    {
        // verificar se foi por tempo ou por ter clicado no errado
        painelDerrota.SetActive(true);
    }

    private void FaseConcluida()
    {
        Debug.Log("Venceu a fase!");
        painelVitoria.SetActive(true);
    }

    public void ReiniciarFase()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
