using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectIngredients : MinigameBase
{
    [Tooltip("Put the ingrediens here")]
    public List<IngredientData> ingredients;


    [Header("Telas de Polimento")]
    public GameObject painelInstrucao;
    public GameObject painelVitoriaFinal;

    public GameObject painelAvisoErro;

    [Header("Visual do Cozinheiro")]
    [Tooltip("Arraste o objeto do Cozinheiro que tem o SpriteRenderer")]
    public SpriteRenderer cozinheiroSpriteRenderer;
    [Tooltip("Coloque aqui o desenho do cozinheiro chorando/triste")]
    public Sprite cozinheiroTristeSprite;


    public ParticleSystem confetesParticulas;
    public float tempoAtrasoVitoria = 1.5f;

    public int currentQuantity;

    // para chamar em outro script
    public static CollectIngredients instance;
    public Transform mesaTransform;
    public GameObject iconPaoPrefab;
    public GameObject iconQueijoPrefab;

    [Header("Controle de Vitória")]
    public int metaPaesDeQueijo = 3;
    private int paesFeitos = 0;

    [HideInInspector]
    public bool jogoIniciado = false;

    // NOVO: vira true no instante em que o jogador faz o último pão de queijo.
    // Antes, o jogo só era considerado "finalizado" DEPOIS da animação
    // (quando Vencer() rodava). Nesse intervalo o timer continuava correndo,
    // o jogador continuava clicando e a RotinaVitoria podia ser disparada
    // várias vezes. Com essa trava, tudo para na hora da vitória.
    private bool vitoriaEmAndamento = false;

    public bool JogoEncerrado => jogoFinalizado || vitoriaEmAndamento;

    private Coroutine rotinaErroAtual;
    private SpawnIngredients spawner;

    void Start()
    {
        instance = this;
        spawner = FindObjectOfType<SpawnIngredients>();

        currentQuantity = ingredients.Count;
        paesFeitos = 0;
        vitoriaEmAndamento = false;

        jogoIniciado = false;
        if (painelInstrucao != null) painelInstrucao.SetActive(true);
        if (painelVitoriaFinal != null) painelVitoriaFinal.SetActive(false);
        if (painelAvisoErro != null) painelAvisoErro.SetActive(false); // Esconde o aviso no começo

        // Congela o jogo (e o timer) até o jogador clicar em "Iniciar"
        Time.timeScale = 0f;
    }

    void OnDestroy()
    {
        // Garante que sair do minigame nunca deixe o jogo congelado
        Time.timeScale = 1f;
        if (instance == this) instance = null;
    }


    public void IniciarMinigame()
    {
        if (jogoIniciado) return; // evita duplo clique no botão

        Time.timeScale = 1f;
        if (painelInstrucao != null) painelInstrucao.SetActive(false);
        jogoIniciado = true;

        // Avisa o Spawner para começar a atirar os ingredientes
        if (spawner != null) spawner.ComecarSpawns();
    }

    // --- INTEGRAÇÃO COM O NOVO CORE ---
    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // Aumenta a quantidade de pães de queijo necessários conforme a fase avança
        metaPaesDeQueijo = Mathf.Min(2 + (faseAtual / 4), 5);
        return tempoGlobalSugerido;
    }

    public void AddIngredient(IngredientData data)
    {
        if (JogoEncerrado || !jogoIniciado) return; // Trava de segurança

        ingredients.Add(data);
        currentQuantity = ingredients.Count;

        GameObject novoIcon = null;

        if (data.nameIngredient == "Pao") novoIcon = iconPaoPrefab;
        else if (data.nameIngredient == "Queijo") novoIcon = iconQueijoPrefab;

        if (novoIcon != null && mesaTransform != null)
        {
            Instantiate(novoIcon, mesaTransform);
        }

        VerifyRevenue();
    }

    public void VerifyRevenue()
    {
        if (JogoEncerrado) return;

        if (ingredients.Count == 2)
        {
            string i1 = ingredients[0].nameIngredient;
            string i2 = ingredients[1].nameIngredient;

            if ((i1 == "Pao" && i2 == "Queijo") || (i1 == "Queijo" && i2 == "Pao"))
            {
                paesFeitos++;
                Debug.Log($"Pão de Queijo Perfeito! ({paesFeitos}/{metaPaesDeQueijo})");
                if (paesFeitos >= metaPaesDeQueijo)
                {
                    IniciarVitoria();
                    return;
                }
            }
            else if (i1 == "Pao" && i2 == "Pao")
            {
                Debug.Log("PÃO DE PÃO! Muito duro!");
                MostrarAvisoErro();
            }
            else if (i1 == "Queijo" && i2 == "Queijo")
            {
                Debug.Log("Muito Mole! Derretido!");
                MostrarAvisoErro();
            }

            LimparMesa();
        }
    }

    private void LimparMesa()
    {
        ingredients.Clear();
        currentQuantity = ingredients.Count;

        if (mesaTransform != null)
        {
            foreach (Transform child in mesaTransform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void MostrarAvisoErro()
    {
        if (rotinaErroAtual != null)
        {
            StopCoroutine(rotinaErroAtual); // Para a rotina anterior se ele errar de novo muito rápido
        }
        rotinaErroAtual = StartCoroutine(RotinaMostrarAviso());
    }

    private IEnumerator RotinaMostrarAviso()
    {
        if (painelAvisoErro != null)
        {
            painelAvisoErro.SetActive(true);
            yield return new WaitForSeconds(1.5f); // Tempo que o alerta de erro fica visível
            painelAvisoErro.SetActive(false);
        }
    }

    // --- SEQUÊNCIA DE VITÓRIA ---
    // 1) trava o jogo na hora  2) toca a animação  3) só então chama Vencer()
    private void IniciarVitoria()
    {
        if (vitoriaEmAndamento) return; // nunca dispara duas vezes
        vitoriaEmAndamento = true;

        LimparMesa();

        // Esconde o aviso de erro caso ele ainda esteja na tela
        if (rotinaErroAtual != null) StopCoroutine(rotinaErroAtual);
        if (painelAvisoErro != null) painelAvisoErro.SetActive(false);

        // Para de chover ingredientes e limpa os que ainda estão voando
        if (spawner != null) spawner.PararSpawns();
        foreach (IngredientBehaviour ing in FindObjectsOfType<IngredientBehaviour>())
        {
            Destroy(ing.gameObject);
        }

        StartCoroutine(RotinaVitoria());
    }

    private IEnumerator RotinaVitoria()
    {
        Vencer();
        if (painelVitoriaFinal != null) painelVitoriaFinal.SetActive(true);
        if (confetesParticulas != null) confetesParticulas.Play();

        yield return new WaitForSeconds(tempoAtrasoVitoria);

        // Segurança extra: se por algum motivo o jogo já terminou, não chama Vencer()
        if (jogoFinalizado) yield break;
    }

    public override void TempoEsgotado()
    {
        // Se o jogador já venceu (animação rolando), o tempo acabar não vale mais.
        // Antes isso gerava derrota + Vencer() ao mesmo tempo.
        if (JogoEncerrado) return;

        if (spawner != null) spawner.PararSpawns();

        // Troca o visual do cozinheiro para triste quando o tempo acaba!
        if (cozinheiroSpriteRenderer != null && cozinheiroTristeSprite != null)
        {
            cozinheiroSpriteRenderer.sprite = cozinheiroTristeSprite;
        }

        base.TempoEsgotado();
    }
}