using System;
using System.Collections;
using System.Collections.Generic;
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
    
    [Header("Transição em íris")]
    public TransicaoIris transicaoIris;
    private Scene cenaIntervalo;

    // Use esta propriedade nos scripts que leem mouse, teclado ou toque.
    public bool PodeJogar => estaJogando && !isGameOver
        && Time.timeScale > 0f
        && (transicaoIris == null || !transicaoIris.EmTransicao);

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
    public TMP_Text txtRegião;
    public GameObject canvaIntervalo, eventos;
    public int pontuacaoJogador = 0;
    public int vidasIniciais = 3;
    private int vidasAtuais;
    public Image[] spritesVidas;

    [Header("Pause")]
    public GameObject pausemenu;
    public AudioSource _audio;
    public GameObject aviaoMenu;
    private string[] regioes = {"Sul", "Sudeste", "Centro-Oeste", "Nordeste", "Norte", "Brasil"};
    private Coroutine ciclar;
    private string regiaoEscolhida;
    async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        cenaIntervalo = gameObject.scene;

        // Start pode executar enquanto a inicialização dos serviços aguarda.
        // Por isso, inicialize o estado local antes do primeiro await.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        if (txtPontuacao != null)
            txtPontuacao.text = "Pontuação: " + pontuacaoJogador;
        vidasAtuais = vidasIniciais;

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
        if (transicaoIris == null || !transicaoIris.EstaPronta)
        {
            Debug.LogError("Configure a TransicaoIris e seu material no Inspector.", this);
            return;
        }
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
            regiaoEscolhida = minigameEscolhido.NomeDaRegiaoFormatado;
            Debug.Log($"Minigame Escolhido: {minigameEscolhido.nomeDoJogo}");
            txtRegião.text = $"Região: {minigameEscolhido.NomeDaRegiaoFormatado}";

            // Estampa a instrução na tela
            if (txtInstrucao != null)
                txtInstrucao.text = minigameEscolhido.instrucao;

            cenaMinigameAtiva = minigameEscolhido.nomeDaCena;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(cenaMinigameAtiva, LoadSceneMode.Additive);
            
            asyncLoad.allowSceneActivation = false; 

            System.GC.Collect();
            yield return StartCoroutine(MoverAviao(minigameEscolhido.NomeDaRegiaoFormatado));
            yield return new WaitForSeconds(3f);

            // Com allowSceneActivation=false, o carregamento espera em 0.9.
            while (asyncLoad.progress < 0.9f)
                yield return null;

            // Captura o intervalo ANTES de escondê-lo ou ativar o minigame.
            // A imagem permanece por cima enquanto a nova cena é preparada.
            yield return transicaoIris.PrepararAbertura();

            canvaIntervalo.SetActive(false);
            if (canvasHUD != null) 
                canvasHUD.SetActive(true);

            asyncLoad.allowSceneActivation = true; 
            
            while (!asyncLoad.isDone)
                yield return null;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(cenaMinigameAtiva));
            AjustarCanvasDaFase(cenaMinigameAtiva);

            MinigameBase minigameAtual = EncontrarMinigameNaCena(
                SceneManager.GetSceneByName(cenaMinigameAtiva));
            
            if (minigameAtual != null)
                timerInterno = minigameAtual.ConfigurarDificuldade(faseAtual, tempoDoMinigameAtual);
            else
                timerInterno = tempoDoMinigameAtual;

            tempoMaximoDaFase = timerInterno;
            Debug.Log(timerInterno);

            timerCongelado = false;
            if (barraTempo != null)
                barraTempo.fillAmount = 1f;

            // Só libera o jogo e o cronômetro depois de abrir completamente.
            yield return transicaoIris.Abrir();
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

            if (!isGameOver && vidasAtuais == vidasInicio)
            {
                int quantosPontos = 0;
                int tipoJogo = (int)minigameEscolhido.tipoJogo;
                if (tipoJogo == 0)
                    quantosPontos = Mathf.Max(1000, Mathf.RoundToInt(5000 * (timerInterno / tempoMaximoDaFase)));
                else if (tipoJogo == 1)
                    quantosPontos = 2000;
                
                pontuacaoJogador += quantosPontos;

                if (txtPontuacao != null)
                    txtPontuacao.text = "Pontuação: " + pontuacaoJogador;

                Debug.Log($"Venceu! Ganhou {quantosPontos} pontos. Total: {pontuacaoJogador}");
            }

            estaJogando = false;
            tempoDoMinigameAtual = Mathf.Max(tempoDoMinigameAtual - decrementoDeTempo, tempoMinimo);

            if (txtInstrucao != null)
                txtInstrucao.gameObject.SetActive(false);

            // Captura o minigame enquanto a cena e o HUD ainda estão visíveis.
            yield return transicaoIris.PrepararFechamento();

            if (canvasHUD != null)
                canvasHUD.SetActive(false);

            // Prepara o intervalo atrás da captura, ainda totalmente opaca.
            SceneManager.SetActiveScene(cenaIntervalo);
            canvaIntervalo.SetActive(true);
            IniciarCiclo();

            if (!string.IsNullOrEmpty(cenaMinigameAtiva))
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(cenaMinigameAtiva);
                if (unload != null)
                    yield return unload;
                cenaMinigameAtiva = "";
            }

            aviaoMenu.SetActive(true);
            // A captura encolhe e revela o intervalo que está por baixo.
            yield return transicaoIris.Fechar();

            if (isGameOver)
            {
                SemVidas();
                yield break;
            }
        }
    }

    private MinigameBase EncontrarMinigameNaCena(Scene cena)
    {
        foreach (GameObject raiz in cena.GetRootGameObjects())
        {
            MinigameBase minigame = raiz.GetComponentInChildren<MinigameBase>();
            if (minigame != null)
                return minigame;
        }
        return null;
    }

    public void VenceuMinigame()
    {
        if (!estaJogando || isGameOver)
            return;
        estaJogando = false;
    }

    public void GameOver()
    {
        // Evita descontar duas vidas por callbacks repetidos da mesma rodada.
        if (!estaJogando || isGameOver)
            return;

        vidasAtuais--;
        if (vidasAtuais >= 0 && vidasAtuais < spritesVidas.Length
            && spritesVidas[vidasAtuais] != null)
            spritesVidas[vidasAtuais].enabled = false;

        estaJogando = false;

        if (vidasAtuais <= 0)
            isGameOver = true; // CicloDeJogo fecha a íris antes de chamar SemVidas.
        else
            Debug.Log("Perdeu uma vida! Restam: " + vidasAtuais);
    }

    private async void SemVidas()
    {
        isGameOver = true;
        // Chamado somente depois que a animação de fechamento terminou.
        StopAllCoroutines();
        estaJogando = false;
        Time.timeScale = 1f;

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

    public void PauseGame()
    {
        if (!PodeJogar)
            return;
        if (Time.timeScale != 1f)
            return;
        Time.timeScale = 0f;
        _audio.Pause();
        Scene cenaCarregada = SceneManager.GetSceneByName(cenaMinigameAtiva);
        GameObject[] objetosRaiz = cenaCarregada.GetRootGameObjects();
        foreach (GameObject obj in objetosRaiz)
        {

            MonoBehaviour[] mb = obj.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour scripts in mb)
            {
                if (scripts != null)
                {
                    if (scripts.GetType() != typeof(TMPro.TextMeshProUGUI) && scripts.GetType() != typeof(CanvasScaler))
                    {
                        if (scripts.GetType() != typeof(Image))
                        {
                            scripts.enabled = false;
                        }
                    }
                }
            }
        }

        pausemenu.SetActive(true);
    }
    public void ResumeGame()
    {
        if (transicaoIris != null && transicaoIris.EmTransicao)
            return;
        if (string.IsNullOrEmpty(cenaMinigameAtiva))
            return;
        Time.timeScale = 1f;
        _audio.Play();
        Scene cenaCarregada = SceneManager.GetSceneByName(cenaMinigameAtiva);
        GameObject[] objetosRaiz = cenaCarregada.GetRootGameObjects();
        foreach (GameObject obj in objetosRaiz)
        {
            MonoBehaviour[] mb = obj.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour scripts in mb)
            {
                if (scripts != null)
                {
                    if (scripts.GetType() != typeof(TMPro.TextMeshProUGUI) && scripts.GetType() != typeof(CanvasScaler))
                    {
                        if (scripts.GetType() != typeof(Image))
                        {
                            scripts.enabled = true;
                        }
                    }
                }
            }
        }

        pausemenu.SetActive(false);
    }

    public void VoltarMenu()
    {
        if (!estaJogando || isGameOver || transicaoIris.EmTransicao)
            return;
        ResumeGame();
        vidasAtuais = 0;
        estaJogando = false;
        isGameOver = true;
        // CicloDeJogo fará o fechamento e então chamará SemVidas.
    }

    IEnumerator MoverAviao(string regiao)
    {
        txtInstrucao.gameObject.SetActive(false);
        aviaoMenu.SetActive(true);
        IniciarCiclo();
        Aviaozinho[] regioes = aviaoMenu.GetComponentsInChildren<Aviaozinho>();
        RectTransform aviao = aviaoMenu.GetComponentInChildren<Image>().gameObject.GetComponent<RectTransform>();
        for (int i = 0; i < regioes.Length; i++)
        {
            if (regioes[i].regiao.ToLower() == regiao.ToLower())
            {
                Vector3 posInicial = aviao.position;
                Vector3 posFinal = regioes[i].gameObject.transform.position;

                Vector3 direcao = regioes[i].transform.position - aviao.position;
                float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
                aviao.rotation = Quaternion.Euler(0, 0, angulo - 90f);
                yield return new WaitForSeconds(0.5f);
                float tempoDecorrido = 0;

                while (tempoDecorrido < 1f)
                {
                    tempoDecorrido += Time.deltaTime;
                    float progresso = tempoDecorrido / 1f;

                    float curvaSuave = Mathf.SmoothStep(0f, 1f, progresso);

                    aviao.position = Vector3.Lerp(posInicial, posFinal, curvaSuave);

                    yield return null;
                }
                aviao.position = posFinal;
            }
        }
        PararCiclo();
        txtRegião.text = $"Região: {regiaoEscolhida}";
        yield return new WaitForSeconds(0.5f);
        aviaoMenu.SetActive(false);
        txtInstrucao.gameObject.SetActive(true);
    }

   private void IniciarCiclo()
    {
        if (txtRegião == null || regioes == null || regioes.Length == 0)
            return;

        txtRegião.gameObject.SetActive(true);

        if (ciclar != null)
            return;

        ciclar = StartCoroutine(CiclarTextos());
    }

    IEnumerator CiclarTextos()
    {
        int indice = UnityEngine.Random.Range(0, regioes.Length);

        while (true)
        {
            txtRegião.text = $"Região: {regioes[indice]}";

            yield return new WaitForSecondsRealtime(0.1f);

            // Evita repetir o mesmo nome em trocas consecutivas.
            if (regioes.Length > 1)
                indice = (indice + UnityEngine.Random.Range(
                    1, regioes.Length)) % regioes.Length;
        }
    }

    public void PararCiclo()
    {
        if (ciclar == null)
            return;

        StopCoroutine(ciclar);
        ciclar = null;
    }
}