using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChineloController : MinigameBase
{
    private ChineloUISpawner chineloSpawner;

    void Start()
    {
        chineloSpawner = Object.FindAnyObjectByType<ChineloUISpawner>();
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;

        if (chineloSpawner.VerificarFimDoJogo())
        {
            Debug.Log("GANHOU no último milissegundo!");
            Vencer();
        }
        else
        {
            Debug.Log("PERDEU! Tempo esgotou com chinelo virado.");
            Perder();
        }
    }

    public void AvisarChineloDesvirado()
    {
        if (jogoFinalizado) return;

        if (chineloSpawner.VerificarFimDoJogo())
        {
            Debug.Log("GANHOU ANTECIPADAMENTE! Jogador foi muito rápido!");
            Vencer();
        }
    }
}