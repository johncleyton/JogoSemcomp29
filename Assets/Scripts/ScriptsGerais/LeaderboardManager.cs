using UnityEngine;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using TMPro;
using System.Text.RegularExpressions;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards.Exceptions;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private string leaderboardId = "top_jogadores";
    [SerializeField] private TMP_Text[] NomeUsuarios;
    [SerializeField] private TMP_Text[] pontuacaoUsuarios;

    public async void MostrarTop3()
    {
        Debug.Log("A transferir o Top 3...");

        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            /*int tentativas = 0;
            while (!AuthenticationService.Instance.IsSignedIn && tentativas < 50)
            {
                await Task.Delay(100); 
                tentativas++;
            }*/

            // Se o Google falhar, regista como anónimo e marca a variável
            // O ranking só consulta os dados depois que o usuário fizer login.
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                for (int j = 0; j < NomeUsuarios.Length; j++)
                {
                    if (NomeUsuarios[j] != null)
                        NomeUsuarios[j].text = j == 0 ? "Faça login para ver o ranking" : "";
                }

                foreach (var texto in pontuacaoUsuarios)
                {
                    if (texto != null)
                        texto.text = "";
                }

                return;
            }

            var opcoesDeBusca = new GetScoresOptions { Limit = 3 };
            LeaderboardScoresPage leaderboardPage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, opcoesDeBusca);
            
            Debug.Log("--- INÍCIO DO TOP 3 ---");
            
            int i = 0;
            foreach (var registro in leaderboardPage.Results)
            {
                if (i >= 3) break; // Garante que não ultrapassa as 3 primeiras posições

                string nomeOriginal = string.IsNullOrEmpty(registro.PlayerName) ? "Jogador Anónimo" : registro.PlayerName;
                
                // MUDANÇA: O \d+ agora remove o # e QUALQUER quantidade de números no final do nome
                string nomeSeguro = Regex.Replace(nomeOriginal, @"#\d+$", "");
                
                if (i < NomeUsuarios.Length) NomeUsuarios[i].text = $"#{registro.Rank+1}. {nomeSeguro}";
                if (i < pontuacaoUsuarios.Length) pontuacaoUsuarios[i].text = registro.Score.ToString();
                
                Debug.Log($"Rank #{registro.Rank + 1} | Jogador: {nomeSeguro} | Pontos: {registro.Score}");
                i++;
            }

            // TRATAMENTO DA 4ª LINHA (Pontuação Pessoal)
            if (NomeUsuarios.Length > 3 && pontuacaoUsuarios.Length > 3)
            {
                try
                {
                    var registroPessoal =
                        await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);

                    string nomePessoal = string.IsNullOrEmpty(registroPessoal.PlayerName)
                        ? "Você"
                        : registroPessoal.PlayerName;

                    string nomePessoalSeguro =
                        Regex.Replace(nomePessoal, @"#\d+$", "");

                    NomeUsuarios[3].text =
                        $"#{registroPessoal.Rank + 1}. {nomePessoalSeguro}";

                    pontuacaoUsuarios[3].text = registroPessoal.Score.ToString();
                }
                catch (LeaderboardsException ex)
                {
                    if (ex.Reason == LeaderboardsExceptionReason.NotFound)
                    {
                        NomeUsuarios[3].text = "Você";
                        pontuacaoUsuarios[3].text = "0";
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Falha ao transferir a Tabela Geral: " + ex.Message);
            
            if (NomeUsuarios.Length > 0) NomeUsuarios[0].text = "Erro de Ligação";
            if (pontuacaoUsuarios.Length > 0) pontuacaoUsuarios[0].text = "X";
        }
    }

    void OnEnable()
    {
        for (int i = 0; i < NomeUsuarios.Length; i++)
        {
            if (NomeUsuarios[i] != null)
                NomeUsuarios[i].text = i < 3 ? "A carregar..." : "";
        }

        for (int i = 0; i < pontuacaoUsuarios.Length; i++)
        {
            if (pontuacaoUsuarios[i] != null)
                pontuacaoUsuarios[i].text = i < 3 ? "-" : "";
        }

        MostrarTop3();
    }
}