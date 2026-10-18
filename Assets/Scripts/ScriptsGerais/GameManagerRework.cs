using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.VisualScripting;
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
    private List<int> minigamesDisponiveis = new List<int>();
    private int ultimoMinigameSorteado = -1;
    
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
    public GameObject pauseHUD;
    public AudioSource _audio;
    public GameObject aviaoMenu;
    private string[] regioes = {"Sul", "Sudeste", "Centro-Oeste", "Nordeste", "Norte", "Brasil"};
    private Coroutine ciclar;
    private string regiaoEscolhida;
    [Header("Fim de Jogo")]
    public GameObject canvaFimDeJogo, canvaDurante;
    public TMP_Text txtScore, txtFases, txtFeedback, txtNome;
    public Animator animator;
    public AudioClip musicaFim;
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
        if (pauseHUD != null)
            pauseHUD.SetActive(false);

        while (true) 
        {            
            faseAtual++;
            
            estaJogando = false;
            canvaIntervalo.SetActive(true);
            if (canvasHUD != null) 
                canvasHUD.SetActive(false);
            if (pauseHUD != null)
                pauseHUD.SetActive(false);

            Debug.Log($"Iniciando Fase {faseAtual}. Prepare-se!");
            
            MinigameData minigameEscolhido = SortearMinigame();
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
            if (pauseHUD != null)
                pauseHUD.SetActive(true);

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
            if (pauseHUD != null)
                pauseHUD.SetActive(false);

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
            
            if (isGameOver)
            {
                canvaDurante.SetActive(false);
                canvaFimDeJogo.SetActive(true);
            }
            else
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

    private MinigameData SortearMinigame()
    {
        if (listaDeMinigames == null || listaDeMinigames.Length == 0)
        {
            Debug.LogError("Nenhum minigame configurado no Inspector.", this);
            return null;
        }

        if (minigamesDisponiveis.Count == 0)
        {
            for (int i = 0; i < listaDeMinigames.Length; i++)
                minigamesDisponiveis.Add(i);
        }

        int posicao = UnityEngine.Random.Range(0, minigamesDisponiveis.Count);

        if (minigamesDisponiveis.Count > 1 && minigamesDisponiveis[posicao] == ultimoMinigameSorteado)
        {
            int deslocamento = UnityEngine.Random.Range(1, minigamesDisponiveis.Count);

            posicao = (posicao + deslocamento) % minigamesDisponiveis.Count;
        }

        int indiceEscolhido = minigamesDisponiveis[posicao];

        minigamesDisponiveis.RemoveAt(posicao);
        ultimoMinigameSorteado = indiceEscolhido;

        return listaDeMinigames[indiceEscolhido];
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
        //canvaDurante.SetActive(false);
        //canvaFimDeJogo.SetActive(true);

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
        _audio.clip = musicaFim;
        _audio.Play();
        _audio.loop = true;
        StartCoroutine(MostrarDadosJogo());
        
        //SceneManager.LoadScene(0);
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
        if (Time.timeScale != 1f || cenaMinigameAtiva == "AcendeFogueira"|| cenaMinigameAtiva == "MinigameBerrante")
            return;
        Time.timeScale = 0f;
        //_audio.Pause();
        _audio.volume = 0.2f;
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
        //_audio.Play();
        _audio.volume = 1f;
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

    public void VoltarMenu2()
    {
        Debug.Log("Clicou");
        StartCoroutine(AnimarTrocar(0));
    }

    public void Restart()
    {
        Debug.Log("Clicou");
        StartCoroutine(AnimarTrocar(1));
    }

    IEnumerator AnimarTrocar(int cena)
    {
        Debug.Log("Entrou corrotina");
        animator.SetTrigger("TrocarTela");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(cena);
    }

    IEnumerator MoverAviao(string regiao)
    {
        txtInstrucao.gameObject.SetActive(false);
        aviaoMenu.SetActive(true);
        IniciarCiclo();
        Aviaozinho[] regioes = aviaoMenu.GetComponentsInChildren<Aviaozinho>();
        RectTransform aviao = GameObject.Find("Aviao").GetComponent<RectTransform>();
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

    IEnumerator MostrarDadosJogo()
    {
        yield return new WaitForSeconds(1f);
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            txtNome.text = "Jogador: não está logado";
        }
        else
        {
            var tarefaNome = AuthenticationService.Instance.GetPlayerNameAsync();
            yield return new WaitUntil(() => tarefaNome.IsCompleted);
            if (tarefaNome.IsFaulted || tarefaNome.IsCanceled)
            {
                txtNome.text = "Jogador: nome indisponível";
                if (tarefaNome.Exception != null)
                    Debug.LogException(tarefaNome.Exception);
            }
            else
            {
                string nome = tarefaNome.Result;

                nome = System.Text.RegularExpressions.Regex.Replace(
                    nome ?? "", @"#\d{5}$", "");

                txtNome.text = string.IsNullOrWhiteSpace(nome)
                    ? "Jogador: nome indisponível"
                    : $"Jogador: {nome}";
            }
        }
        yield return new WaitForSeconds(1f);
        txtFases.text = $"Fases jogadas: {faseAtual.ToString()}";
        yield return new WaitForSeconds(1f);
        yield return ContarPontuacao(pontuacaoJogador);
        yield return new WaitForSeconds(1f);
        if (pontuacaoJogador < 10000)
            txtFeedback.text = "Jogou de olho fechado?";
        else if (pontuacaoJogador < 25000)
            txtFeedback.text = "Sambou, sambou e dançou";
        else if (pontuacaoJogador < 50000)
            txtFeedback.text = "Tá pegando o ritmo";
        else if (pontuacaoJogador > 72700 && pontuacaoJogador < 72799)
            txtFeedback.text = "WYSI!!!!";
        else if (pontuacaoJogador < 100000)
            txtFeedback.text = "Você tem o molho brasileiro";
        else if (pontuacaoJogador < 200000)
            txtFeedback.text = "O jeitinho brasileiro vive em você";
        else if (pontuacaoJogador < 250000)
            txtFeedback.text = "Como? Por que?";
        else
            txtFeedback.text = "Oh Lee, você já provou que tem o molho";

        yield return null;
    }

    private IEnumerator ContarPontuacao(int pontuacaoFinal)
    {
        if (txtScore == null)
            yield break;

        if (pontuacaoFinal == 0)
        {
            txtScore.text = $"Pontuação final: {pontuacaoFinal}";
            yield break;
        }

        float tempo = 0f;
        float duracao = 2f;

        txtScore.text = "Pontuação final: 0";
        yield return null;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracao);
            float progressoSuave = 1f - Mathf.Pow(1f - progresso, 3f);

            int pontuacaoExibida = Mathf.RoundToInt(
                Mathf.Lerp(0f, pontuacaoFinal, progressoSuave)
            );

            txtScore.text = $"Pontuação final: {pontuacaoExibida}";

            yield return null;
        }

        txtScore.text = $"Pontuação final: {pontuacaoFinal}";
    }
}