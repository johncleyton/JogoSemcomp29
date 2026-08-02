using UnityEngine;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models; // Biblioteca necessária para ler a estrutura de resultados
using System.Threading.Tasks;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private string leaderboardId = "top_jogadores"; 

    public async void EnviarPontuacao(int novaPontuacao)
    {
        Debug.Log($"Enviando pontuação: {novaPontuacao} para o Leaderboard...");
        try
        {
            var resposta = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, novaPontuacao);
            Debug.Log($"Sucesso! Pontuação atualizada no servidor. Recorde salvo: {resposta.Score}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Falha ao enviar pontuação: " + ex.Message);
        }
    }

    public async void MostrarTop3()
    {
        Debug.Log("Baixando o Top 3...");
        try
        {
            var opcoesDeBusca = new GetScoresOptions { Limit = 3 };
            LeaderboardScoresPage leaderboardPage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, opcoesDeBusca);
            
            Debug.Log("--- INÍCIO DO TOP 3 ---");
            
            foreach (var registro in leaderboardPage.Results)
                Debug.Log($"Rank #{registro.Rank + 1} | Jogador: {registro.PlayerName} | Score: {registro.Score}");
            
            Debug.Log("--- FIM DO TOP 3 ---");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Falha ao baixar o Leaderboard: " + ex.Message);
        }
    }
}