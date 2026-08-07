using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float timer = 0;
    int sceneCount = 0;
    public int qtdBrigadeiro = 4;

    async void Awake()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("UGS Inicializado com sucesso.");
        }
        catch (Exception e)
        {
            Debug.LogError("Erro UGS: " + e.Message);
        }
    }
    // Start is called before the first frame update
    void Start()
    {

        sceneCount = SceneManager.sceneCountInBuildSettings;
        //Debug.Log(sceneCount);
        timer = GlobalVariables.timer;
        //Debug.Log(timer);

        qtdBrigadeiro = UnityEngine.Random.Range(Mathf.RoundToInt(10 - timer), Mathf.RoundToInt(12 - timer));
        //Debug.Log(qtdBrigadeiro);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            //Debug.Log(timer);
        }
        else
        {
            int random = UnityEngine.Random.Range(1, sceneCount);
            GlobalVariables.timer = Mathf.Max((float)(GlobalVariables.timer - 0.1), (float)3.0);
            GlobalVariables.qualFase += 1;
            SceneManager.LoadScene(random);
        }
    }

    async public void GameOver()
    {
        int novaPontuacao = GlobalVariables.qualFase;
        Debug.Log($"Enviando pontuação: {novaPontuacao} para o Leaderboard...");
        try
        {
            var resposta = await LeaderboardsService.Instance.AddPlayerScoreAsync("top_jogadores", novaPontuacao);
            Debug.Log($"Sucesso! Pontuação atualizada no servidor. Recorde salvo: {resposta.Score}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Falha ao enviar pontuação: " + ex.Message);
        }
    }
}
