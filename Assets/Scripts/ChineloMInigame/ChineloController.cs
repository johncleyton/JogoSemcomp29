using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChineloController : MonoBehaviour
{
    private GameManager gameManager;
    private ChineloUISpawner chineloSpawner;
    private bool resultadoAvaliado = false;

    void Start()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>();
        chineloSpawner = Object.FindAnyObjectByType<ChineloUISpawner>();
    }

    void Update()
    {
        if (gameManager == null || chineloSpawner == null || resultadoAvaliado) return;

        if (gameManager.timer <= 0.05f)
        {
            resultadoAvaliado = true;
            if (chineloSpawner.VerificarFimDoJogo())
            {
                Debug.Log("GANHOU no último milissegundo!");
            }
            else
            {
                Debug.Log("PERDEU! Tempo esgotou com chinelo virado.");
            }
        }
    }
    public void AvisarChineloDesvirado()
    {
        if (resultadoAvaliado) return;

        if (chineloSpawner.VerificarFimDoJogo())
        {
            resultadoAvaliado = true;
            Debug.Log("GANHOU ANTECIPADAMENTE! Jogador foi muito rápido!");

            GlobalVariables.timer += 1f;
            gameManager.timer = 0.1f;
        }
    }
}