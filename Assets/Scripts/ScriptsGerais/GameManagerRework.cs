using System;
using System.Collections;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerRework : MonoBehaviour
{
    public static GameManagerRework Instance { get; private set; }

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
    
    private string cenaMinigameAtiva = ""; 

    // Variáveis de controle de fluxo
    private bool estaJogando = false;
    private float timerInterno = 0f;
    private bool isGameOver = false;
    public bool timerCongelado = false;

    [Header("Minigames")]
    public MinigameData[] listaDeMinigames; 
    
    [Header("UI")]
    public TMP_Text txtInstrucao;
    public TMP_Text txtPontuacao;
    public GameObject canvaIntervalo, eventos;

    public int pontuacaoJogador = 0;


    public int vidasIniciais = 3;
    private int vidasAtuais;
    public Image[] spritesVidas;

    async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("UGS Inicializado com sucesso.");
        }
        catch (Exception e)
        {
            Debug.LogError("Erro UGS: " + e.Message);
        }

        txtPontuacao.text = "Pontuação: " + pontuacaoJogador;

        vidasAtuais = vidasIniciais;
    }

    void Start()
    {
        StartCoroutine(CicloDeJogo());
    }

    IEnumerator CicloDeJogo()
    {
        if (canvasHUD != null) 
            canvasHUD.SetActive(false);

        while (true) 
        {            
            faseAtual++;
            
            estaJogando = false;
            canvaIntervalo.SetActive(true);
            if (canvasHUD != null) 
                canvasHUD.SetActive(false);

            Debug.Log($"Iniciando Fase {faseAtual}. Prepare-se!");
            
            int randomSorteio = UnityEngine.Random.Range(0, listaDeMinigames.Length);
            MinigameData minigameEscolhido = listaDeMinigames[randomSorteio];

            Debug.Log($"Minigame Escolhido: {minigameEscolhido.nomeDoJogo}");


            // Estampa a instrução na tela
            if (txtInstrucao != null)
                txtInstrucao.text = minigameEscolhido.instrucao;

            cenaMinigameAtiva = minigameEscolhido.nomeDaCena;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(cenaMinigameAtiva, LoadSceneMode.Additive);
            
            asyncLoad.allowSceneActivation = false; 

            System.GC.Collect();
            yield return new WaitForSeconds(3f);
            
            canvaIntervalo.SetActive(false);
            if (canvasHUD != null) 
                canvasHUD.SetActive(true);

            asyncLoad.allowSceneActivation = true; 
            
            while (!asyncLoad.isDone)
                yield return null;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(cenaMinigameAtiva));
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

            int vidasInicio = vidasAtuais;

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

            if (vidasAtuais == vidasInicio)
            {
                int quantosPontos = 0;
                if (minigameEscolhido.tipoJogo == 0)
                    quantosPontos = Mathf.Max(100, Mathf.RoundToInt(5000 * (timerInterno / tempoMaximoDaFase)));
                else if (minigameEscolhido.tipoJogo == 1)
                    quantosPontos = 2500;
                
                pontuacaoJogador += quantosPontos;

                if (txtPontuacao != null)
                    txtPontuacao.text = "Pontuação: " + pontuacaoJogador;

                Debug.Log($"Venceu! Ganhou {quantosPontos} pontos. Total: {pontuacaoJogador}");
            }

            estaJogando = false;
            tempoDoMinigameAtual = Mathf.Max(tempoDoMinigameAtual - decrementoDeTempo, tempoMinimo);

            if (!string.IsNullOrEmpty(cenaMinigameAtiva))
                SceneManager.UnloadSceneAsync(cenaMinigameAtiva);
        }
    }

    public void VenceuMinigame()
    {
        estaJogando = false; 
    }

    public async void GameOver()
    {
        vidasAtuais--;
        spritesVidas[vidasAtuais].enabled = false;

        estaJogando = false;

        if (vidasAtuais <= 0)
            SemVidas();
        else
            Debug.Log("Perdeu uma vida! Restam: " + vidasAtuais);
    }

    private async void SemVidas()
    {
        isGameOver = true;
        StopAllCoroutines(); 
        estaJogando = false;

        Debug.Log($"Game Over Definitivo! Enviando pontuação: {pontuacaoJogador} para o Leaderboard...");
        try
        {
            // Alterado para enviar a pontuacaoJogador em vez da faseAtual
            var resposta = await LeaderboardsService.Instance.AddPlayerScoreAsync("top_jogadores", pontuacaoJogador); 
            Debug.Log($"Recorde salvo: {resposta.Score}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Falha ao enviar pontuação: " + ex.Message);
        }
        
        SceneManager.LoadScene(0);
    }

    private void AjustarCanvasDaFase(string nomeDaCena)
    {
        Scene cenaCarregada = SceneManager.GetSceneByName(nomeDaCena);
        GameObject[] objetosRaiz = cenaCarregada.GetRootGameObjects();
        
        foreach (GameObject obj in objetosRaiz)
        {
            Canvas[] canvases = obj.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main; 
            }

            CanvasScaler[] scalers = obj.GetComponentsInChildren<CanvasScaler>(true);
            foreach (CanvasScaler scaler in scalers)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(800, 600); 
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0f; 
            }
        }
    }
}