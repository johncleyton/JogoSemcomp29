using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;

public class TosquieManager : MinigameBase
{
    public static TosquieManager Instance;

    [Header("Fila de ovelhas")]
    [Tooltip("Prefab da Ovelha (Assets/Prefabs/TosquieAOvelha/Ovelha).")]
    public GameObject ovelhaPrefab;
    [Tooltip("Onde cada ovelha nova para. Pode usar o objeto 'Centro'.")]
    public Transform pontoSpawnOvelha;

    [Header("Transição entre ovelhas")]
    [Tooltip("Quão longe (em unidades) a ovelha começa/termina fora da tela. Tem que ser maior que a metade da largura da câmera.")]
    public float distanciaEntrada = 14f;
    public float duracaoTransicao = 0.45f;

    [Header("Tosquiadora")]
    public TosquiadoraController tosquiadora;

    [Header("UI - Placar e status")]
    public TMP_Text placarText;
    public TMP_Text tentativasText;

    [Header("UI - Feedback (opcional)")]
    [Tooltip("Texto central curto: 'Pente errado!', 'Tosquiada!', 'Escapou!'. Pode deixar vazio.")]
    public TMP_Text feedbackText;
    public Color corAcerto = new Color(0.4f, 1f, 0.5f);
    public Color corErro = new Color(1f, 0.35f, 0.3f);

    [Header("Clareza pra jogadores novos (passo a passo)")]
    [Tooltip("Faixa de instrução embaixo: PASSO 1 (escolha) e PASSO 2 (clique). Vazio = criada sozinha.")]
    public TMP_Text instrucaoText;
    public bool mostrarInstrucao = true;
    public string textoPasso1 = "1/2  QUE OVELHA É ESSA? ESCOLHA O BOTÃO";
    public string textoPasso2 = "2/2  CLIQUE PARA TOSQUIAR!";
    public Color corPasso2 = new Color(1f, 0.92f, 0.3f);
    [Tooltip("Cria um rótulo de texto ao lado de cada botão (os ícones sozinhos confundem).")]
    public bool criarRotulosDosBotoes = true;
    public string rotuloPenteMenor = "ADULTA";
    public string rotuloPenteMaior = "VELHA";
    public string rotuloDescarta = "NOVA\n(passar)";
    [Tooltip("Botões pulsam de leve enquanto esperam a escolha; ficam apagados depois.")]
    public bool pulsarBotoes = true;
    [Range(0f, 1f)] public float alphaBotoesInativos = 0.3f;

    [Header("UI - Botões dos pentes")]
    public Button botaoPenteMenor;   // Ovelha EmPerfeitoEstado -> Pente Menor
    public Button botaoPenteMaior;   // Ovelha MuitoVelha        -> Pente Maior
    public Button botaoDescarta;     // Ovelha MuitoNova         -> Descarta

    [Header("Configurações")]
    [FormerlySerializedAs("totalFatias")]
    [Tooltip("Quantas ovelhas precisam ser tosquiadas corretamente para vencer o minigame.")]
    public int ovelhasNecessarias = 3;
    [Tooltip("Quantas vezes a tosquiadora pode cair sem acertar lã, por ovelha, antes dela escapar.")]
    public int tentativasPorOvelha = 3;
    [Tooltip("Quantas tentativas escolher o pente ERRADO custa. 0 = sem penalidade (dá pra chutar à vontade).")]
    public int penalidadeErroPente = 1;

    [Tooltip("Descartar corretamente uma ovelha NOVA conta como ovelha concluída.")]
    public bool descarteCorretoConta = true;

    [Header("Alcance da tosquiadora (ajustado sozinho pela área da lã da ovelha)")]
    public bool ajustarAlcanceAutomatico = true;
    [Tooltip("Quanto a tosquiadora passa pra fora da lã, pros dois lados.")]
    public float margemHorizontal = 0.5f;
    [Tooltip("Até onde ela cai abaixo da base da lã antes de contar como erro.")]
    public float margemQueda = 0f;

    [Header("Dificuldade")]
    public bool escalarComFase = true;
    [Tooltip("Quanto a velocidade da tosquiadora aumenta por fase (0.06 = +6%).")]
    public float aumentoVelocidadePorFase = 0.06f;
    public float multiplicadorMaximo = 1.8f;

    [Header("Tempo (o tempo global sugerido, 7s caindo até 3s, é curto demais pra este minigame)")]
    [Tooltip("Segundos estimados pra escolher o pente de cada ovelha.")]
    public float tempoPorEscolha = 1.5f;
    [Tooltip("Segundos estimados pra cortar cada pedaço de lã (inclui erros).")]
    public float tempoPorPedaco = 1.8f;
    [Tooltip("Quanto o tempo total diminui a cada fase.")]
    public float reducaoTempoPorFase = 0.15f;
    public float tempoMinimoTosquie = 8f;

    private OvelhaController ovelhaAtual;
    private EstadoOvelha ultimoEstado = EstadoOvelha.EmPerfeitoEstado;
    private int pedacosRestantesNaOvelha;
    private int ovelhasConcluidas = 0;
    private int tentativasRestantes;
    private bool aguardandoEscolha = false; // false enquanto a ovelha está entrando/saindo
    private bool tosquiando = false;
    private Coroutine feedbackRot;
    private readonly System.Collections.Generic.List<KeyValuePair<Button, TextMeshProUGUI>> rotulos =
        new System.Collections.Generic.List<KeyValuePair<Button, TextMeshProUGUI>>();

    public bool JogoEncerrado => jogoFinalizado;
    public bool FaseDeTosquia => tosquiando;

    void Awake()
    {
        Instance = this;
        GarantirEventSystem();
    }

    // Rodando a cena isolada não existe EventSystem (quem tem é a TelaIntervalo) e os botões não respondem.
    // Integrado ao jogo, o da TelaIntervalo já está carregado e nada é criado aqui.
    void GarantirEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;

        GameObject go = new GameObject("EventSystem (criado pelo TosquieManager)");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        SceneManager.MoveGameObjectToScene(go, gameObject.scene);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (botaoPenteMenor != null) botaoPenteMenor.onClick.RemoveListener(EscolherPenteMenor);
        if (botaoPenteMaior != null) botaoPenteMaior.onClick.RemoveListener(EscolherPenteMaior);
        if (botaoDescarta != null) botaoDescarta.onClick.RemoveListener(EscolherDescarta);
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        int nivel = Mathf.Max(0, faseAtual);

        if (escalarComFase && tosquiadora != null)
            tosquiadora.multiplicadorVelocidade = Mathf.Min(1f + aumentoVelocidadePorFase * nivel, multiplicadorMaximo);

        // O tempo global (7s caindo até 3s) não dá pra tosquiar várias ovelhas: calcula um tempo próprio
        // a partir do nº de ovelhas e de pedaços de lã do prefab.
        int pedacos = ovelhaPrefab != null ? ContarPedacosDeLa(ovelhaPrefab) : 3;
        if (pedacos == 0 && ovelhaPrefab != null)
        {
            // As faixas agora podem ser criadas por código na hora, então o prefab pode não tê-las salvas
            OvelhaController oc = ovelhaPrefab.GetComponent<OvelhaController>();
            pedacos = oc != null ? oc.quantidadeFaixas : 3;
        }
        float tempo = ovelhasNecessarias * (tempoPorEscolha + Mathf.Max(1, pedacos) * tempoPorPedaco);
        if (escalarComFase) tempo -= reducaoTempoPorFase * nivel;

        return Mathf.Max(tempo, tempoMinimoTosquie);
    }

    void Start()
    {
        ovelhasConcluidas = 0;
        AtualizarPlacar();
        GarantirFeedbackText();
        if (feedbackText != null) feedbackText.text = "";
        GarantirInstrucaoText();
        DefinirInstrucao("", Color.white);

        if (criarRotulosDosBotoes)
        {
            CriarRotulo(botaoPenteMenor, rotuloPenteMenor);
            CriarRotulo(botaoPenteMaior, rotuloPenteMaior);
            CriarRotulo(botaoDescarta, rotuloDescarta);
        }
        StartCoroutine(AnimarInterface());

        if (botaoPenteMenor != null) botaoPenteMenor.onClick.AddListener(EscolherPenteMenor);
        if (botaoPenteMaior != null) botaoPenteMaior.onClick.AddListener(EscolherPenteMaior);
        if (botaoDescarta != null) botaoDescarta.onClick.AddListener(EscolherDescarta);

        if (tosquiadora != null) tosquiadora.PodeAgir = false;

        NovaOvelha();
    }

    // ------------------------------------------------------------------
    // Fila de ovelhas
    // ------------------------------------------------------------------

    void NovaOvelha()
    {
        if (ovelhaPrefab == null)
        {
            Debug.LogError("TosquieManager: 'Ovelha Prefab' não foi atribuído no Inspector.");
            return;
        }

        Vector3 destino = pontoSpawnOvelha != null ? pontoSpawnOvelha.position : Vector3.zero;
        Vector3 inicio = destino + Vector3.right * distanciaEntrada;

        GameObject instancia = Instantiate(ovelhaPrefab, inicio, Quaternion.identity);
        // Instantiate cria na cena ATIVA, que pode ser a TelaIntervalo (minigame carregado aditivamente).
        // Sem isso a ovelha sobra na cena quando o minigame é descarregado.
        SceneManager.MoveGameObjectToScene(instancia, gameObject.scene);

        ovelhaAtual = instancia.GetComponent<OvelhaController>();
        if (ovelhaAtual == null)
        {
            Debug.LogError("TosquieManager: o prefab da Ovelha não tem o componente OvelhaController.");
            Destroy(instancia);
            return;
        }

        ovelhaAtual.Inicializar(SortearEstado());

        pedacosRestantesNaOvelha = ContarPedacosDeLa(instancia);
        if (pedacosRestantesNaOvelha <= 0)
            Debug.LogWarning("TosquieManager: a ovelha instanciada não tem nenhum filho com a tag 'La'.");

        tentativasRestantes = tentativasPorOvelha;
        AtualizarTentativas();

        aguardandoEscolha = false;
        tosquiando = false;
        DefinirBotoesInterativos(false); // só libera depois que a ovelha para
        if (tosquiadora != null) tosquiadora.PodeAgir = false;

        StartCoroutine(EntrarOvelha(ovelhaAtual, destino));
    }

    IEnumerator EntrarOvelha(OvelhaController ovelha, Vector3 destino)
    {
        yield return ovelha.Entrar(destino, duracaoTransicao);

        if (jogoFinalizado || ovelha != ovelhaAtual) yield break;

        aguardandoEscolha = true;
        DefinirBotoesInterativos(true);
        DefinirInstrucao(textoPasso1, Color.white);
    }

    // Evita sortear o mesmo estado várias vezes seguidas (fica repetitivo)
    EstadoOvelha SortearEstado()
    {
        EstadoOvelha novo = (EstadoOvelha)Random.Range(0, 3);
        if (novo == ultimoEstado && Random.value < 0.5f)
            novo = (EstadoOvelha)(((int)novo + Random.Range(1, 3)) % 3);
        ultimoEstado = novo;
        return novo;
    }

    int ContarPedacosDeLa(GameObject raiz)
    {
        int total = 0;
        foreach (Transform filho in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (filho.CompareTag("La")) total++;
        }
        return total;
    }

    // ------------------------------------------------------------------
    // Escolha do pente
    // ------------------------------------------------------------------

    public void EscolherPenteMenor() => ProcessarEscolha(EstadoOvelha.EmPerfeitoEstado);
    public void EscolherPenteMaior() => ProcessarEscolha(EstadoOvelha.MuitoVelha);
    public void EscolherDescarta() => ProcessarEscolha(EstadoOvelha.MuitoNova);

    void ProcessarEscolha(EstadoOvelha estadoEscolhido)
    {
        if (jogoFinalizado || !aguardandoEscolha || ovelhaAtual == null) return;

        if (estadoEscolhido != ovelhaAtual.estadoAtual)
        {
            ErrouPente();
            return;
        }

        // Ovelha nova + botão Descarta = acertou: ela NÃO é tosquiada, só passa pra próxima.
        // (Antes o descarte correto iniciava a tosquia de uma ovelha que não tem lã.)
        if (ovelhaAtual.estadoAtual == EstadoOvelha.MuitoNova) DescartarOvelha();
        else IniciarTosquia();
    }

    void DescartarOvelha()
    {
        MostrarFeedback("Descartada!", corAcerto);

        if (descarteCorretoConta) ConcluirOvelha(false);
        else EncerrarOvelhaAtual();
    }

    void ErrouPente()
    {
        ovelhaAtual.Balancar();
        MostrarFeedback("Pente errado!", corErro);

        if (penalidadeErroPente > 0)
        {
            tentativasRestantes -= penalidadeErroPente;
            AtualizarTentativas();

            if (tentativasRestantes <= 0)
            {
                MostrarFeedback("Escapou!", corErro);
                EncerrarOvelhaAtual();
                return;
            }
        }

        StartCoroutine(FeedbackErro());
    }

    IEnumerator FeedbackErro()
    {
        DefinirBotoesInterativos(false);
        yield return new WaitForSeconds(0.35f);
        if (!jogoFinalizado && aguardandoEscolha && ovelhaAtual != null)
            DefinirBotoesInterativos(true);
    }

    void IniciarTosquia()
    {
        aguardandoEscolha = false;
        tosquiando = true;
        DefinirBotoesInterativos(false);
        DefinirInstrucao(textoPasso2, corPasso2);

        if (tosquiadora != null)
        {
            if (ovelhaAtual != null)
            {
                tosquiadora.DefinirFerramenta(ovelhaAtual.estadoAtual);

                // Ajusta o vai-e-vem e a queda à lã da ovelha: sem isso ela varre ±2.5 mesmo que a lã
                // esteja em outro lugar, e cai atravessando a ovelha até o fim da tela.
                if (ajustarAlcanceAutomatico)
                {
                    tosquiadora.limiteEsquerdo = ovelhaAtual.EsquerdaDaLaX - margemHorizontal;
                    tosquiadora.limiteDireito = ovelhaAtual.DireitaDaLaX + margemHorizontal;
                    tosquiadora.limiteErroY = ovelhaAtual.BaseDaLaY - margemQueda;
                }
            }
            tosquiadora.ResetarParaTopo();
            tosquiadora.PodeAgir = true;
        }
    }

    void DefinirBotoesInterativos(bool valor)
    {
        AplicarEstadoBotao(botaoPenteMenor, valor);
        AplicarEstadoBotao(botaoPenteMaior, valor);
        AplicarEstadoBotao(botaoDescarta, valor);
    }

    // Botão ativo = opaco e clicável; inativo = apagado (e o rótulo, que é filho, apaga junto)
    void AplicarEstadoBotao(Button b, bool ativo)
    {
        if (b == null) return;
        b.interactable = ativo;

        CanvasGroup cg = b.GetComponent<CanvasGroup>();
        if (cg == null) cg = b.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = ativo ? 1f : alphaBotoesInativos;
    }

    // ------------------------------------------------------------------
    // Resultado da tosquia (chamado pela TosquiadoraController)
    // ------------------------------------------------------------------

    public void CortouLa()
    {
        if (jogoFinalizado) return;

        pedacosRestantesNaOvelha--;
        if (ovelhaAtual != null) ovelhaAtual.Pulinho();

        if (pedacosRestantesNaOvelha > 0)
        {
            // ainda tem lã nesta ovelha: mostra o progresso (o guia pede um contador visível)
            MostrarFeedback("Faltam " + pedacosRestantesNaOvelha + "!", corAcerto);
            return;
        }

        MostrarFeedback("Tosquiada!", corAcerto);
        ConcluirOvelha(true);
    }

    // Conta a ovelha como resolvida (tosquiada ou descartada corretamente) e vence se bateu a meta
    void ConcluirOvelha(bool tosquiada)
    {
        ovelhasConcluidas++;
        AtualizarPlacar();
        if (tosquiada && ovelhaAtual != null) ovelhaAtual.MostrarTosquiada();

        if (ovelhasConcluidas >= ovelhasNecessarias)
        {
            if (tosquiadora != null) tosquiadora.PodeAgir = false;
            aguardandoEscolha = false;
            tosquiando = false;
            DefinirBotoesInterativos(false);
            DefinirInstrucao("", Color.white);
            Vencer();
            return;
        }

        EncerrarOvelhaAtual();
    }

    public void TentativaFalhou()
    {
        if (jogoFinalizado) return;

        tentativasRestantes--;
        AtualizarTentativas();

        if (tentativasRestantes <= 0)
        {
            MostrarFeedback("Escapou!", corErro);
            EncerrarOvelhaAtual();
        }
    }

    void EncerrarOvelhaAtual()
    {
        if (tosquiadora != null) tosquiadora.PodeAgir = false;
        aguardandoEscolha = false;
        tosquiando = false;
        DefinirBotoesInterativos(false);
        DefinirInstrucao("", Color.white);

        if (ovelhaAtual != null)
        {
            // A ovelha velha sai pela esquerda enquanto a nova entra pela direita (sem sobreposição)
            ovelhaAtual.SairDaTela(Vector3.left * distanciaEntrada, duracaoTransicao);
            ovelhaAtual = null;
        }
        NovaOvelha();
    }

    // ------------------------------------------------------------------
    // UI
    // ------------------------------------------------------------------

    void AtualizarPlacar()
    {
        if (placarText != null)
            placarText.text = $"Ovelhas: {ovelhasConcluidas}/{ovelhasNecessarias}";
    }

    void AtualizarTentativas()
    {
        if (tentativasText != null)
            tentativasText.text = $"Tentativas: {Mathf.Max(0, tentativasRestantes)}";
    }

    // ------------------------------------------------------------------
    // Clareza pra jogador novo: faixa de passo a passo + rótulos nos botões
    // ------------------------------------------------------------------

    void GarantirInstrucaoText()
    {
        if (instrucaoText != null || placarText == null || placarText.transform.parent == null) return;

        GameObject go = new GameObject("InstrucaoText", typeof(RectTransform));
        go.transform.SetParent(placarText.transform.parent, false);

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 45f);
        rt.sizeDelta = new Vector2(1500f, 90f);

        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = 44;
        t.fontStyle = FontStyles.Bold;
        t.raycastTarget = false;
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(0, 0, 0, 255);
        t.text = "";

        instrucaoText = t;
    }

    void DefinirInstrucao(string texto, Color cor)
    {
        if (instrucaoText == null) return;
        instrucaoText.text = mostrarInstrucao ? texto : "";
        instrucaoText.color = cor;
    }

    // O rótulo é filho do CANVAS (não do botão): os botões têm escala não uniforme e o texto filho
    // herdava a distorção (letras espremidas e sobrepostas). Ele só segue a posição do botão.
    void CriarRotulo(Button b, string texto)
    {
        if (b == null || string.IsNullOrEmpty(texto)) return;

        Transform pai = (placarText != null && placarText.transform.parent != null) ? placarText.transform.parent : b.transform.parent;
        GameObject go = new GameObject("Rotulo_" + b.name, typeof(RectTransform));
        go.transform.SetParent(pai, false);

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(360f, 100f);

        t.alignment = TextAlignmentOptions.MidlineRight;
        t.enableWordWrapping = false;
        t.fontSize = 38;
        t.fontStyle = FontStyles.Bold;
        t.color = Color.white;
        t.raycastTarget = false;
        t.outlineWidth = 0.25f;
        t.outlineColor = new Color32(0, 0, 0, 255);
        t.text = texto.Replace("(passar)", "<size=70%>(passar)</size>");

        rotulos.Add(new KeyValuePair<Button, TextMeshProUGUI>(b, t));
        PosicionarRotulos();
    }

    // Cola cada rótulo na borda esquerda do botão e copia a transparência dele (roda todo quadro)
    void PosicionarRotulos()
    {
        Vector3[] cantos = new Vector3[4];
        for (int i = 0; i < rotulos.Count; i++)
        {
            Button b = rotulos[i].Key;
            TextMeshProUGUI t = rotulos[i].Value;
            if (b == null || t == null) continue;

            ((RectTransform)b.transform).GetWorldCorners(cantos); // 0 = baixo-esq, 1 = cima-esq
            Vector3 esquerdaMeio = (cantos[0] + cantos[1]) * 0.5f;
            float escCanvas = t.canvas != null ? t.canvas.transform.lossyScale.x : 1f;
            t.rectTransform.position = esquerdaMeio + Vector3.left * (14f * escCanvas);

            CanvasGroup cg = b.GetComponent<CanvasGroup>();
            t.alpha = cg != null ? cg.alpha : 1f;
        }
    }

    // Pulso leve nos botões esperando a escolha e na faixa do PASSO 2 (chama a atenção sem animação pronta)
    IEnumerator AnimarInterface()
    {
        Button[] botoes = { botaoPenteMenor, botaoPenteMaior, botaoDescarta };
        Vector3[] baseEscala = new Vector3[botoes.Length];
        for (int i = 0; i < botoes.Length; i++)
            baseEscala[i] = botoes[i] != null ? botoes[i].transform.localScale : Vector3.one;

        while (true)
        {
            PosicionarRotulos();
            float onda = Mathf.Sin(Time.time * 5f);
            float kBotao = (pulsarBotoes && aguardandoEscolha && !jogoFinalizado) ? 1f + 0.06f * onda : 1f;

            for (int i = 0; i < botoes.Length; i++)
                if (botoes[i] != null) botoes[i].transform.localScale = baseEscala[i] * kBotao;

            if (instrucaoText != null)
            {
                float kTexto = (tosquiando && !jogoFinalizado) ? 1f + 0.05f * Mathf.Sin(Time.time * 8f) : 1f;
                instrucaoText.transform.localScale = Vector3.one * kTexto;
            }

            yield return null;
        }
    }

    // Se 'Feedback Text' ficou vazio, cria um texto central no mesmo Canvas do placar
    // (usa o Canvas do próprio minigame, não o da TelaIntervalo).
    void GarantirFeedbackText()
    {
        if (feedbackText != null || placarText == null || placarText.transform.parent == null) return;

        GameObject go = new GameObject("FeedbackText", typeof(RectTransform));
        go.transform.SetParent(placarText.transform.parent, false);

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        RectTransform rt = t.rectTransform;
        // embaixo, no meio: em cima ele ficava por cima da tosquiadora
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 170f);
        rt.sizeDelta = new Vector2(1000f, 140f);

        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = 80;
        t.fontStyle = FontStyles.Bold;
        t.raycastTarget = false;
        t.text = "";

        feedbackText = t;
    }

    void MostrarFeedback(string msg, Color cor)
    {
        if (feedbackText == null) return;
        if (feedbackRot != null) StopCoroutine(feedbackRot);
        feedbackRot = StartCoroutine(FeedbackRoutine(msg, cor));
    }

    IEnumerator FeedbackRoutine(string msg, Color cor)
    {
        feedbackText.text = msg;
        feedbackText.color = cor;
        yield return new WaitForSeconds(0.5f);

        const float dur = 0.3f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            feedbackText.color = new Color(cor.r, cor.g, cor.b, 1f - t / dur);
            yield return null;
        }
        feedbackText.text = "";
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;
        MostrarFeedback("Acabou o tempo!", corErro);
        DefinirInstrucao("", Color.white);
        base.TempoEsgotado(); // a base já chama Perder()
    }
}