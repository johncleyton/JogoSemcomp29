using System;
using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerRework : MonoBehaviour
{
    // Permite que qualquer minigame acesse o GameManager facilmente
    // a partir do singleton (lina omg!!!!)
    public static GameManagerRework Instance { get; private set; }

    // Antigas variaveis globais
    [Header("Game State")]
    public float tempoDoMinigameAtual = 7f;
    public int faseAtual = 0;               
    
    [Header("Barra de tempo")]
    public GameObject canvasHUD;
    private float tempoMaximoDaFase; 
    public Image barraTempo;

    [Header("Configurações")]
    public float tempoMinimo = 3.0f;
    public float decrementoDeTempo = 0.1f;
    
    private int sceneCount = 0;
    private int cenaMinigameAtiva = -1; // Guarda o índice da cena do minigame que está rodando

    // Variáveis de controle de fluxo
    private bool estaJogando = false;
    private float timerInterno = 0f;
    private bool isGameOver = false;
    public bool timerCongelado = false;

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
        if (canvasHUD != null) 
            canvasHUD.SetActive(false);

        while (true) // Loop infinito até dar GameOver
        {
            faseAtual++;
            
            estaJogando = false;
            canvaIntervalo.SetActive(true);
            if (canvasHUD != null) 
                canvasHUD.SetActive(false);

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
            if (canvasHUD != null) 
                canvasHUD.SetActive(true);

            asyncLoad.allowSceneActivation = true; // Ativa a cena carregada
            cenaMinigameAtiva = randomScene;
            
            while (!asyncLoad.isDone)
                yield return null;

            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(cenaMinigameAtiva));

            AjustarCanvasDaFase(cenaMinigameAtiva);

            MinigameBase minigameAtual = FindObjectOfType<MinigameBase>();
            
            if (minigameAtual != null)
                timerInterno = minigameAtual.ConfigurarDificuldade(faseAtual, tempoDoMinigameAtual);
            else
                timerInterno = tempoDoMinigameAtual;

            tempoMaximoDaFase = timerInterno;

            Debug.Log(timerInterno);

            timerCongelado = false;
            estaJogando = true;
            bool tempoEsgotadoAcionado = false;

            while (estaJogando)
            {
                if (!timerCongelado && !tempoEsgotadoAcionado)
                {
                    timerInterno -= Time.deltaTime;
                    if (barraTempo != null)
                        barraTempo.fillAmount = timerInterno / tempoMaximoDaFase;

                    if (timerInterno <= 0)
                    {
                        tempoEsgotadoAcionado = true;
                        if (minigameAtual != null)
                            minigameAtual.TempoEsgotado();
                        else
                            GameOver();
                    }
                }
                yield return null;
            }

            if (isGameOver)
            {
                yield break; 
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
        isGameOver = true;
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

    private void AjustarCanvasDaFase(int indexDaCena)
    {
        // Pega a cena do minigame que acabou de carregar
        Scene cenaCarregada = SceneManager.GetSceneByBuildIndex(indexDaCena);
        
        // Pega todos os objetos soltos na raiz dessa cena
        GameObject[] objetosRaiz = cenaCarregada.GetRootGameObjects();
        
        foreach (GameObject obj in objetosRaiz)
        {
            // Prende todos os Canvas do minigame na Câmera Principal
            Canvas[] canvases = obj.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main; 
            }

            // Força o Scaler a usar a sua proporção original de 800x600
            CanvasScaler[] scalers = obj.GetComponentsInChildren<CanvasScaler>(true);
            foreach (CanvasScaler scaler in scalers)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(800, 600); 
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0f; // O valor que alinhou a UI perfeitamente com a física
            }
        }
    }
}