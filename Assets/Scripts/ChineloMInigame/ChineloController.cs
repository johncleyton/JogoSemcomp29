using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChineloController : MinigameBase
{
    private ChineloUISpawner chineloSpawner;

    public GameObject EventSystem;

    public GameObject Feedback;

    public Sprite Win;
    public Sprite Lose;

    [Header("Configuração")]
    public float tempoDeFeedback = 2f;

    void Start()
    {
        chineloSpawner = Object.FindAnyObjectByType<ChineloUISpawner>();
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;

        if (chineloSpawner.VerificarFimDoJogo())
        {
            MostrarFeedback(true);
            VencerComAtraso(tempoDeFeedback);
        }
        else
        {
            MostrarFeedback(false);
            PerderComAtraso(tempoDeFeedback);
        }
    }

    public void AvisarChineloDesvirado()
    {
        if (jogoFinalizado) return;

        if (chineloSpawner.VerificarFimDoJogo())
        {
            Debug.Log("GANHOU ANTECIPADAMENTE! Jogador foi muito rápido!");
            MostrarFeedback(true);
            VencerComAtraso(tempoDeFeedback);
        }
    }

    private void MostrarFeedback(bool vitoria)
    {
        EventSystem.SetActive(false);

        Feedback.GetComponent<Image>().sprite = vitoria ? Win : Lose;
        Feedback.SetActive(true);
    }
}