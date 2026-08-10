using System;
using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManagerRework : MonoBehaviour
{
    // Permite que qualquer minigame acesse o GameManager facilmente
    // a partir do singleton (lina omg!!!!)
    public static GameManagerRework Instance { get; private set; }

    // Antigas variaveis globais
    [Header("Game State")]
    public float tempoDoMinigameAtual = 7f; 
    public int faseAtual = 0;               
    
    [Header("Configurações")]
    public float tempoMinimo = 3.0f;
    public float decrementoDeTempo = 0.1f;
    
    private int sceneCount = 0;
    private int cenaMinigameAtiva = -1; // Guarda o índice da cena do minigame que está rodando

    // Variáveis de controle de fluxo
    private bool estaJogando = false;
    private float timerInterno = 0f;

    public TMP_Text txtFase;
    public GameObject canvaIntervalo, eventos;
    async void Awake()
    {
        // Configuração do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Inicializa o banco de dados para alterar a pontuação ao fim de jogo
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

    void Start()
    {
        // Subtrai 1 se a cena desse manager estiver nas settings
        sceneCount = SceneManager.sceneCountInBuildSettings; 
        Debug.Log($"Quantas cenas: {sceneCount}");
        
        // Inicia o loop do jogo
        StartCoroutine(CicloDeJogo());
    }

    IEnumerator CicloDeJogo()
    {
        while (true) // Loop infinito até dar GameOver
        {
            faseAtual++;
            
            estaJogando = false;
            canvaIntervalo.SetActive(true);

            Debug.Log($"Iniciando Fase {faseAtual}. Prepare-se!");
            
            // Sorteia e começa a carregar o próximo minigame em segundo plano usando o LoadSceneAsync()
            int randomScene = UnityEngine.Random.Range(2, sceneCount);
            Debug.Log($"Cena escolhida: {randomScene}");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(randomScene, LoadSceneMode.Additive);
            asyncLoad.allowSceneActivation = false; // Nao carrega de imediato por causa da transicao

            // Limpa lixo de memória para rodar mais fluido
            System.GC.Collect();

            // Espera o tempo da animação do intervalo, tanto faz o tempo
            yield return new WaitForSeconds(3f);
            canvaIntervalo.SetActive(false);

            asyncLoad.allowSceneActivation = true; // Ativa a cena carregada
            cenaMinigameAtiva = randomScene;
            
            //while (!asyncLoad.isDone)
                yield return null;

            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(cenaMinigameAtiva));

            MinigameBase minigameAtual = FindObjectOfType<MinigameBase>();
            
            if (minigameAtual != null)
                timerInterno = minigameAtual.ConfigurarDificuldade(faseAtual, tempoDoMinigameAtual);
            else
                timerInterno = tempoDoMinigameAtual;

            Debug.Log(timerInterno);

            estaJogando = true;

            // Espera o tempo acabar
            while (timerInterno > 0 && estaJogando)
            {
                //Debug.Log(timerInterno);
                timerInterno -= Time.deltaTime;
                yield return null; // Espera o próximo frame
            }
            
            if (estaJogando && timerInterno <= 0)
            {
                estaJogando = false;
                if (minigameAtual != null)
                    minigameAtual.TempoEsgotado();
                else
                    GameOver();
            }

            // Acabou o tempo, avisa que o jogador nao pode mais jogar
            estaJogando = false;
            
            // Atualiza o timer pro próximo minigame
            tempoDoMinigameAtual = Mathf.Max(tempoDoMinigameAtual - decrementoDeTempo, tempoMinimo);

            // Descarrega o minigame que acabou de ser jogado
            if (cenaMinigameAtiva != -1)
                SceneManager.UnloadSceneAsync(cenaMinigameAtiva);
        }
    }

    public void VenceuMinigame()
    {
        // Um minigame chama essa função e para o timer antes
        estaJogando = false; 
    }

    public async void GameOver()
    {
        StopAllCoroutines(); // Para o loop do jogo
        estaJogando = false;

        Debug.Log($"Game Over! Enviando pontuação: {faseAtual} para o Leaderboard...");
        try
        {
            var resposta = await LeaderboardsService.Instance.AddPlayerScoreAsync("top_jogadores", faseAtual);
            Debug.Log($"Recorde salvo: {resposta.Score}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Falha ao enviar pontuação: " + ex.Message);
        }
        
        SceneManager.LoadScene(0);

    }
}