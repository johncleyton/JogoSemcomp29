using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;

public class PulaFogueiraController : MinigameBase
{
    private float minigameDuration;

    public void Start()
    {
        minigameDuration = Random.Range(4, 8);
    }

    public override void TempoEsgotado()
    {
        if(jogoFinalizado)
        {
            return;
        }
        VencerComAtraso(0f);
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        return minigameDuration;
    }

    public void playerHit()
    {
        Debug.Log("Perdeu: bateu na fogueira");
        PerderComAtraso(0f);
    }

    public float getMinigameDuration()
    {
        return minigameDuration;
    }
}