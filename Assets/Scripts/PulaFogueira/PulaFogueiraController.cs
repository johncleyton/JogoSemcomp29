using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;

public class PulaFogueiraController : MinigameBase
{
    private float minigameDuration;

    public void Start()
    {
        minigameDuration = Random.Range(3, 13);
    }

    public override void TempoEsgotado()
    {
        if(jogoFinalizado)
        {
            return;
        }
        Vencer();
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        return minigameDuration;
    }

    public void playerHit()
    {
        Debug.Log("Perdeu: bateu na fogueira");
        Perder();
    }

    public float getMinigameDuration()
    {
        return minigameDuration;
    }
}