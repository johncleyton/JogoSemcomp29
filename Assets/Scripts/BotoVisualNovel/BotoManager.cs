using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BotoManager : MinigameBase
{
    public static BotoManager Instance;

    [Header("UI - Textos do Visual Novel")]
    public TMP_Text contextoText;      // Narração
    public TMP_Text falaPotiraText;    // Fala da Potira

    [Tooltip("Opcional. Mostra 'Corações: X/5'. Pode deixar vazio no Inspector se não for usar.")]
    public TMP_Text progressoText;

    [Header("UI - Botões de Resposta (3: Boa / Neutra / Ruim)")]
    public Button[] botoesResposta;    // Arraste os 3 botões para cá no Inspector
    public TMP_Text[] textosBotoes;    // Arraste os 3 textos dos botões para cá

    [Header("Feedback Visual da Potira")]
    public SpriteRenderer potiraSpriteRenderer;
    public Sprite potiraIdle;
    [FormerlySerializedAs("potiraExtrema")] // mantém o que já estava arrastado no Inspector
    public Sprite potiraFeliz;             // potyra_feliz.png  (resposta Boa)
    public Sprite potiraNeutra;            // potyra_neutra.png (resposta Neutra)
    public Sprite potiraBrava;             // potyra_brava.png  (resposta Ruim)

    [Header("Feedback de Corações")]
    [Tooltip("Particle System (Shuriken) configurado no Editor com um material/sprite de coração. O código só chama Play().")]
    public ParticleSystem coracoesParticleSystem;

    [Header("Efeito de Digitação")]
    [Tooltip("Caracteres por segundo. 0 = o texto aparece de uma vez.")]
    public float caracteresPorSegundo = 120f;

    [Header("Configuração de Tempo (usado em ConfigurarDificuldade)")]
    [Tooltip("Tempo mínimo de leitura + decisão por rodada, em segundos.")]
    public float tempoMinimoPorRodada = 5f;
    [Tooltip("Tempo aproximado das animações de reação da Potira por rodada, em segundos.")]
    public float tempoReacaoPorRodada = 1.2f;
    [Tooltip("Tempo aproximado do texto sendo digitado por rodada, em segundos.")]
    public float tempoDigitacaoPorRodada = 2f;

    private const int NUMERO_RODADAS = 5;

    // Durações das reações (devem caber em tempoReacaoPorRodada)
    private const float DURACAO_REACAO_BOA = 1.1f;
    private const float DURACAO_REACAO_NEUTRA = 0.8f;
    private const float DURACAO_REACAO_RUIM = 0.9f;

    // Linhas de narração mostradas logo após a escolha do jogador
    private const string REACAO_BOA = "Potira cora, leva as mãos ao rosto e sorri, desviando o olhar para o rio.";
    private const string REACAO_NEUTRA = "Potira assente com a cabeça, sem muita emoção.";
    private const string REACAO_RUIM = "Potira cruza os braços e vira o rosto, ofendida.";

    public enum TipoResposta { Boa, Neutra, Ruim }

    private struct OpcaoResposta
    {
        public string texto;
        public TipoResposta tipo;

        public OpcaoResposta(string texto, TipoResposta tipo)
        {
            this.texto = texto;
            this.tipo = tipo;
        }
    }

    private class RodadaDialogo
    {
        public string contexto;
        public string falaPotira;
        public string respostaBoa;
        public string respostaNeutra;
        public string respostaRuim;

        public RodadaDialogo(string contexto, string falaPotira, string boa, string neutra, string ruim)
        {
            this.contexto = contexto;
            this.falaPotira = falaPotira;
            this.respostaBoa = boa;
            this.respostaNeutra = neutra;
            this.respostaRuim = ruim;
        }
    }

    private List<RodadaDialogo> rotaAtual;
    private int rodadaAtualIndex = 0;
    private int coracoesConquistados = 0;

    // Controle de estado
    private bool digitando = false;
    private bool reacaoEmAndamento = false;
    private bool finalEmAndamento = false; // depois da última escolha válida, o timer não pode mais derrotar o jogador

    // Para as animações da Potira voltarem ao normal
    private Vector3 escalaOriginalPotira = Vector3.one;
    private Vector3 posOriginalPotira = Vector3.zero;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // Esta visual novel roda 5 rodadas sequenciais dentro da mesma partida. Garantimos o piso
        // mínimo de tempo para ler, digitar, decidir e reagir em TODAS as rodadas, mesmo que o
        // GameManagerRework sugira um tempo curto para a fase atual.
        float digitacao = caracteresPorSegundo > 0f ? tempoDigitacaoPorRodada : 0f;
        float tempoMinimoTotal = NUMERO_RODADAS * (tempoMinimoPorRodada + tempoReacaoPorRodada + digitacao);
        return Mathf.Max(tempoGlobalSugerido, tempoMinimoTotal);
    }

    void Start()
    {
        if (!ValidarReferencias()) return;

        if (potiraSpriteRenderer != null)
        {
            escalaOriginalPotira = potiraSpriteRenderer.transform.localScale;
            posOriginalPotira = potiraSpriteRenderer.transform.localPosition;
        }

        TrocarSprite(potiraIdle);

        rotaAtual = MontarRota();
        rodadaAtualIndex = 0;
        coracoesConquistados = 0;

        ExibirRodada(rodadaAtualIndex);
    }

    // Avisa no Console o que ficou sem arrastar no Inspector (em vez de falhar em silêncio).
    private bool ValidarReferencias()
    {
        bool ok = true;

        if (contextoText == null) { Debug.LogError("[BotoManager] 'Contexto Text' não foi atribuído no Inspector.", this); ok = false; }
        if (falaPotiraText == null) { Debug.LogError("[BotoManager] 'Fala Potira Text' não foi atribuído no Inspector.", this); ok = false; }

        if (botoesResposta == null || botoesResposta.Length < 3)
        {
            Debug.LogError("[BotoManager] Arraste os 3 botões em 'Botoes Resposta'.", this);
            ok = false;
        }
        else if (textosBotoes == null || textosBotoes.Length < botoesResposta.Length)
        {
            Debug.LogError("[BotoManager] 'Textos Botoes' precisa ter um texto para cada botão.", this);
            ok = false;
        }

        if (potiraSpriteRenderer == null)
        {
            Debug.LogWarning("[BotoManager] 'Potira Sprite Renderer' vazio: arraste o objeto PotiraSprite. Sem isso ela aparece como quadrado branco.", this);
        }
        else
        {
            if (potiraIdle == null) Debug.LogWarning("[BotoManager] Sprite 'Potira Idle' vazio.", this);
            if (potiraFeliz == null) Debug.LogWarning("[BotoManager] Sprite 'Potira Feliz' vazio.", this);
            if (potiraNeutra == null) Debug.LogWarning("[BotoManager] Sprite 'Potira Neutra' vazio.", this);
            if (potiraBrava == null) Debug.LogWarning("[BotoManager] Sprite 'Potira Brava' vazio.", this);
        }

        return ok;
    }

    private List<RodadaDialogo> MontarRota()
    {
        // Roteiro fixo: aniversário de Potira à beira do Rio Negro (ver roteiro-visual-novel.pdf).
        // A introdução (Rio Negro / apresentação da Potira / é aniversário dela) foi condensada
        // no contexto da Rodada 1 para não precisar de uma "rodada 0" fora do loop de decisões.
        return new List<RodadaDialogo>
        {
            new RodadaDialogo(
                "Rio Negro, Amazonas. Potira, jovem lavradeira encantadora, faz aniversário hoje — e o Boto sabe bem disso. " +
                "Já é noite: a jovem lavradeira parece cabisbaixa olhando para o rio, solitária em sua própria festa. " +
                "O Boto se aproxima para iniciar uma conversa.",

                "\"Tanta gente nessa festa, mas as águas do rio parecem a minha única companhia hoje...\"",

                "Então permita que eu seja a maré que vai alegrar a sua noite.",

                "A festa está animada, você devia aproveitar mais.",

                "Reclamando no próprio aniversário? Que chatice."
            ),
            new RodadaDialogo(
                "Potira se vira para o Boto, seus olhos parecem que brilham. Ela está em um vestido, trazendo uma flor potyra " +
                "repousada na orelha direita.",
                "\"Colhi essa flor potyra agora há pouco... Acha que combinou com meu vestido?\"",
                "A flor é linda, mas sua beleza ofusca qualquer pétala, Potira.",
                "Sim, combinou bastante com o estilo.",
                "Achei meio grande demais para o seu rosto."
            ),
            new RodadaDialogo(
                "Potira suspira pesadamente enquanto esfrega um tecido mais grosso contra a bancada de madeira.",
                "\"Tem algumas marcas nesse tecido que parecem que nunca vão sair, não importa o quanto eu esfregue...\"",
                "Com paciência e carinho, até as marcas mais difíceis desaparecem, flor.",
                "É só deixar de molho no rio que amanhã fica mais fácil de limpar.",
                "Talvez você devesse esfregar com mais vontade em vez de reclamar."
            ),
            new RodadaDialogo(
                "A tarde começa a se despedir e a iluminação do cenário diminui gradativamente sobre as águas do Rio Negro.",
                "\"O sol já está indo embora... O dia do meu aniversário passou tão rápido trabalhando aqui.\"",
                "O dia pode até acabar, mas a noite guarda as melhores surpresas para você.",
                "Os dias passam rápido mesmo quando temos tanta roupa para lavar.",
                "Ainda bem, já estava na hora de você parar de resmungar e descansar."
            ),
            new RodadaDialogo(
                "Potira finalmente larga a última peça de roupa limpa e olha fixamente nos olhos do Boto, intrigada com a presença dele.",
                "\"Você ficou aqui me fazendo companhia o tempo todo... Por que se importa tanto com uma simples lavradeira?\"",
                "Porque para mim, você é a joia mais preciosa das águas do Amazonas.",
                "Eu estava apenas de passagem e não tinha nada melhor para fazer hoje.",
                "Alguém tinha que vigiar para ver se você lavava tudo direito."
            ),
        };
    }

    // ---------------------------------------------------------------------
    // Exibição de rodada
    // ---------------------------------------------------------------------

    private void ExibirRodada(int index)
    {
        StartCoroutine(RotinaExibirRodada(index));
    }

    private IEnumerator RotinaExibirRodada(int index)
    {
        digitando = true;
        RodadaDialogo rodada = rotaAtual[index];

        OcultarBotoes();
        AtualizarProgresso();

        contextoText.text = rodada.contexto;
        falaPotiraText.text = rodada.falaPotira;

        // Narração e fala "digitam" ao mesmo tempo; os botões só aparecem depois.
        yield return StartCoroutine(DigitarTextos(contextoText, falaPotiraText));

        List<OpcaoResposta> banco = new List<OpcaoResposta>
        {
            new OpcaoResposta(rodada.respostaBoa, TipoResposta.Boa),
            new OpcaoResposta(rodada.respostaNeutra, TipoResposta.Neutra),
            new OpcaoResposta(rodada.respostaRuim, TipoResposta.Ruim),
        };

        EmbaralharEAtribuirRespostas(banco);
        digitando = false;
    }

    private IEnumerator DigitarTextos(params TMP_Text[] textos)
    {
        int maxCaracteres = 0;
        foreach (TMP_Text t in textos)
        {
            t.maxVisibleCharacters = 0;
            maxCaracteres = Mathf.Max(maxCaracteres, t.text.Length);
        }

        if (caracteresPorSegundo > 0f)
        {
            float tempo = 0f;
            int visiveis = 0;
            while (visiveis < maxCaracteres)
            {
                tempo += Time.deltaTime;
                visiveis = Mathf.FloorToInt(tempo * caracteresPorSegundo);
                foreach (TMP_Text t in textos) t.maxVisibleCharacters = visiveis;
                yield return null;
            }
        }

        foreach (TMP_Text t in textos) t.maxVisibleCharacters = 99999;
    }

    private void EmbaralharEAtribuirRespostas(List<OpcaoResposta> respostas)
    {
        // Fisher-Yates, para evitar o speedrun de "sempre o botão do meio".
        for (int i = 0; i < respostas.Count; i++)
        {
            OpcaoResposta temp = respostas[i];
            int randomIndex = Random.Range(i, respostas.Count);
            respostas[i] = respostas[randomIndex];
            respostas[randomIndex] = temp;
        }

        for (int i = 0; i < botoesResposta.Length; i++)
        {
            if (i >= respostas.Count)
            {
                // Segurança: se sobrar algum botão a mais arrastado no Inspector, ele é ocultado.
                botoesResposta[i].gameObject.SetActive(false);
                continue;
            }

            botoesResposta[i].gameObject.SetActive(true);
            botoesResposta[i].interactable = true;
            textosBotoes[i].text = respostas[i].texto;

            // Variável local para não bugar o índice/tipo no closure do listener.
            TipoResposta tipoClosure = respostas[i].tipo;

            botoesResposta[i].onClick.RemoveAllListeners();
            botoesResposta[i].onClick.AddListener(() => AvaliarEscolha(tipoClosure));
        }
    }

    // ---------------------------------------------------------------------
    // Escolha e reação
    // ---------------------------------------------------------------------

    public void AvaliarEscolha(TipoResposta tipo)
    {
        if (jogoFinalizado || digitando || reacaoEmAndamento) return;

        // Desativa todos os botões imediatamente para o jogador não dar "duplo clique"
        foreach (Button btn in botoesResposta)
        {
            if (btn != null) btn.interactable = false;
        }

        StartCoroutine(RotinaDeReacao(tipo));
    }

    private IEnumerator RotinaDeReacao(TipoResposta tipo)
    {
        reacaoEmAndamento = true;
        OcultarBotoes();

        bool ultimaRodada = (rodadaAtualIndex == NUMERO_RODADAS - 1);

        // RUIM: Game Over imediato, em qualquer rodada.
        if (tipo == TipoResposta.Ruim)
        {
            contextoText.text = REACAO_RUIM;
            TrocarSprite(potiraBrava);
            StartCoroutine(TremerPotira(0.5f));
            yield return new WaitForSeconds(DURACAO_REACAO_RUIM);

            Perder(); // a Potira continua brava durante o delay padrão do Perder()
            yield break;
        }

        bool acertou = (tipo == TipoResposta.Boa);

        // A partir daqui o resultado da partida está garantido: o timer não pode mais derrotar.
        if (ultimaRodada) finalEmAndamento = true;

        if (acertou)
        {
            coracoesConquistados++;
            AtualizarProgresso();
            contextoText.text = REACAO_BOA;

            if (coracoesParticleSystem != null) coracoesParticleSystem.Play();

            TrocarSprite(potiraFeliz);
            StartCoroutine(PulsarPotira(0.4f));
            yield return new WaitForSeconds(DURACAO_REACAO_BOA);
        }
        else // NEUTRA: avança sem gerar corações, mas nunca é Game Over.
        {
            contextoText.text = REACAO_NEUTRA;
            TrocarSprite(potiraNeutra);
            yield return new WaitForSeconds(DURACAO_REACAO_NEUTRA);
        }

        if (ultimaRodada)
        {
            // Só a Escolha 5 "Boa" leva à "transformação final completa". Chegar ao fim com
            // "Neutra" ainda conta como vitória (nunca dá Game Over).
            yield return StartCoroutine(ExibirFinal(acertou));
            Vencer();
            yield break;
        }

        TrocarSprite(potiraIdle);
        rodadaAtualIndex++;
        reacaoEmAndamento = false;
        ExibirRodada(rodadaAtualIndex);
    }

    private IEnumerator ExibirFinal(bool transformacaoCompleta)
    {
        AtualizarProgresso();

        if (transformacaoCompleta)
        {
            contextoText.text = "Sob o luar refletido no Rio Negro, o Boto se transforma diante de Potira, " +
                "selando o encontro que começou naquela tarde.";
            falaPotiraText.text = "\"Eu sabia que essa noite guardava algo especial...\"";
            TrocarSprite(potiraFeliz);
        }
        else
        {
            contextoText.text = "A noite chega ao fim tranquila. Não houve grandes declarações, " +
                "mas a companhia foi bem-vinda.";
            falaPotiraText.text = "\"Obrigada por ficar comigo hoje, forasteiro.\"";
            TrocarSprite(potiraIdle);
        }

        yield return StartCoroutine(DigitarTextos(contextoText, falaPotiraText));
        yield return new WaitForSeconds(1.5f);
    }

    // ---------------------------------------------------------------------
    // Helpers visuais
    // ---------------------------------------------------------------------

    private void TrocarSprite(Sprite sprite)
    {
        if (potiraSpriteRenderer != null && sprite != null)
            potiraSpriteRenderer.sprite = sprite;
    }

    private void AtualizarProgresso()
    {
        if (progressoText != null)
            progressoText.text = $"Corações: {coracoesConquistados}/{NUMERO_RODADAS}";
    }

    private void OcultarBotoes()
    {
        if (botoesResposta == null) return;
        foreach (Button btn in botoesResposta)
        {
            if (btn != null) btn.gameObject.SetActive(false);
        }
    }

    // Pulinho de alegria (escala sobe e volta)
    private IEnumerator PulsarPotira(float duracao)
    {
        if (potiraSpriteRenderer == null) yield break;

        Transform t = potiraSpriteRenderer.transform;
        float tempo = 0f;
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float escala = 1f + 0.08f * Mathf.Sin((tempo / duracao) * Mathf.PI);
            t.localScale = escalaOriginalPotira * escala;
            yield return null;
        }
        t.localScale = escalaOriginalPotira;
    }

    // Tremidinha de raiva (balança no eixo X e vai diminuindo)
    private IEnumerator TremerPotira(float duracao)
    {
        if (potiraSpriteRenderer == null) yield break;

        Transform t = potiraSpriteRenderer.transform;
        float tempo = 0f;
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float atenuacao = 1f - (tempo / duracao);
            t.localPosition = posOriginalPotira + Vector3.right * Mathf.Sin(tempo * 50f) * 0.15f * atenuacao;
            yield return null;
        }
        t.localPosition = posOriginalPotira;
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;

        // Se o jogador já fez a escolha final (Boa ou Neutra), o final está rodando: não pune.
        if (finalEmAndamento) return;

        // Não escolheu a tempo: Game Over automático (comportamento padrão do MinigameBase).
        base.TempoEsgotado();
    }
}