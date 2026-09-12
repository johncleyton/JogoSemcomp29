using UnityEngine;
using TMPro; // Biblioteca necessária para os textos bonitos
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
    public Sprite potiraExtrema;
    public Sprite potiraNeutra;

    [Header("Feedback de Corações")]
    [Tooltip("Particle System (Shuriken) configurado no Editor com um material/sprite de coração. O código só chama Play().")]
    public ParticleSystem coracoesParticleSystem;

    [Header("Configuração de Tempo (usado em ConfigurarDificuldade)")]
    [Tooltip("Tempo mínimo de leitura + decisão por rodada, em segundos.")]
    public float tempoMinimoPorRodada = 5f;
    [Tooltip("Tempo aproximado das animações de reação da Potira por rodada, em segundos.")]
    public float tempoReacaoPorRodada = 1.2f;

    private const int NUMERO_RODADAS = 5;

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

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // Diferente de um minigame instantâneo de 2-5s, esta visual novel roda 5 rodadas
        // sequenciais dentro da mesma partida. Garantimos o piso mínimo de tempo necessário
        // para o jogador ler e decidir em TODAS as rodadas, mesmo que o GameManagerRework
        // sugira um tempo curto para a fase atual.
        float tempoMinimoTotal = NUMERO_RODADAS * (tempoMinimoPorRodada + tempoReacaoPorRodada);
        return Mathf.Max(tempoGlobalSugerido, tempoMinimoTotal);
    }

    void Start()
    {
        if (potiraSpriteRenderer != null) potiraSpriteRenderer.sprite = potiraIdle;

        rotaAtual = MontarRota();
        rodadaAtualIndex = 0;
        coracoesConquistados = 0;

        ExibirRodada(rodadaAtualIndex);
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

    private void ExibirRodada(int index)
    {
        RodadaDialogo rodada = rotaAtual[index];

        contextoText.text = rodada.contexto;
        falaPotiraText.text = rodada.falaPotira;

        if (progressoText != null)
            progressoText.text = $"Corações: {coracoesConquistados}/{NUMERO_RODADAS}";

        List<OpcaoResposta> banco = new List<OpcaoResposta>
        {
            new OpcaoResposta(rodada.respostaBoa, TipoResposta.Boa),
            new OpcaoResposta(rodada.respostaNeutra, TipoResposta.Neutra),
            new OpcaoResposta(rodada.respostaRuim, TipoResposta.Ruim),
        };

        EmbaralharEAtribuirRespostas(banco);
    }

    private void EmbaralharEAtribuirRespostas(List<OpcaoResposta> respostas)
    {
        // Fisher-Yates, igual ao original, para evitar o speedrun de "sempre o botão do meio".
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

    public void AvaliarEscolha(TipoResposta tipo)
    {
        if (jogoFinalizado) return;

        // Desativa todos os botões imediatamente para o jogador não dar "duplo clique"
        foreach (Button btn in botoesResposta)
        {
            if (btn != null) btn.interactable = false;
        }

        StartCoroutine(RotinaDeReacao(tipo));
    }

    private IEnumerator RotinaDeReacao(TipoResposta tipo)
    {
        // RUIM: Game Over imediato, em qualquer rodada.
        if (tipo == TipoResposta.Ruim)
        {
            if (potiraSpriteRenderer != null) potiraSpriteRenderer.sprite = potiraExtrema;
            yield return new WaitForSeconds(0.6f);
            if (potiraSpriteRenderer != null) potiraSpriteRenderer.sprite = potiraNeutra;
            yield return new WaitForSeconds(0.4f);

            Perder();
            yield break;
        }

        bool acertou = (tipo == TipoResposta.Boa);

        if (acertou)
        {
            coracoesConquistados++;
            if (coracoesParticleSystem != null) coracoesParticleSystem.Play();

            if (potiraSpriteRenderer != null) potiraSpriteRenderer.sprite = potiraExtrema;
            yield return new WaitForSeconds(0.6f);
        }
        else // NEUTRA: avança sem gerar corações, mas nunca é Game Over.
        {
            if (potiraSpriteRenderer != null) potiraSpriteRenderer.sprite = potiraNeutra;
            yield return new WaitForSeconds(0.5f);
        }

        if (potiraSpriteRenderer != null) potiraSpriteRenderer.sprite = potiraIdle;
        yield return new WaitForSeconds(0.2f);

        bool ultimaRodada = (rodadaAtualIndex == NUMERO_RODADAS - 1);

        if (ultimaRodada)
        {
            // Só a Escolha 5 "Boa" leva à "transformação final completa". Chegar ao fim com
            // "Neutra" ainda conta como vitória (nunca dá Game Over)
            // bônus de pontos garantidos na vitória "Boa"
            yield return StartCoroutine(ExibirFinal(acertou));
            Vencer();
        }
        else
        {
            rodadaAtualIndex++;
            ExibirRodada(rodadaAtualIndex);
        }
    }

    private IEnumerator ExibirFinal(bool transformacaoCompleta)
    {
        if (progressoText != null)
            progressoText.text = $"Corações: {coracoesConquistados}/{NUMERO_RODADAS}";

        if (transformacaoCompleta)
        {
            contextoText.text = "Sob o luar refletido no Rio Negro, o Boto se transforma diante de Potira, " +
            "selando o encontro que começou naquela tarde.";
            falaPotiraText.text = "\"Eu sabia que essa noite guardava algo especial...\"";
        }
        else
        {
            contextoText.text = "A noite chega ao fim tranquila. Não houve grandes declarações, " +
            "mas a companhia foi bem-vinda.";
            falaPotiraText.text = "\"Obrigada por ficar comigo hoje, forasteiro.\"";
        }

        yield return new WaitForSeconds(1.5f);
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;

        // não clicou a tempo, é GameOver automático.
        Perder();
        base.TempoEsgotado();
    }
}
