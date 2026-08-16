using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using System.Threading.Tasks;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private string leaderboardId = "top_jogadores";
    public async void FinalizarMinigame(int pontuacaoFinal)
    {
        Debug.Log("Game Over! Pontuação final: " + pontuacaoFinal);

        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Salvando score na nuvem para o jogador: " + AuthenticationService.Instance.PlayerId);
            await EnviarScoreParaNuvem(pontuacaoFinal);
        }
        else
        {
            Debug.LogWarning("O jogador está jogando como anônimo/deslogado. O score não será salvo.");
        }
    }

    private async Task EnviarScoreParaNuvem(int score)
    {
        try
        {
            var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            Debug.Log($"Sucesso! Score de {score} registrado no placar.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao salvar o score: " + e.Message);
        }
    }
}