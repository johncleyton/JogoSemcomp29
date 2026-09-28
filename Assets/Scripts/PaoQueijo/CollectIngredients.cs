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
    
    public bool JogoEncerrado => jogoFinalizado;

    private Coroutine rotinaErroAtual;

    void Start()
    {
        instance = this;
        currentQuantity = ingredients.Count;

        jogoIniciado = false;
        if (painelInstrucao != null) painelInstrucao.SetActive(true);
        if(painelVitoriaFinal != null) painelVitoriaFinal.SetActive(false);
        if (painelAvisoErro != null) painelAvisoErro.SetActive(false); // Esconde o aviso no começo
        
    }


    public void IniciarMinigame()
    {
        if (painelInstrucao != null) painelInstrucao.SetActive(false);
        jogoIniciado = true;
        
        // Avisa o Spawner para começar a atirar os ingredientes
        SpawnIngredients spawner = FindObjectOfType<SpawnIngredients>();
        if (spawner != null) spawner.ComecarSpawns();
    }

    // --- INTEGRAÇÃO COM O NOVO CORE ---
    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // Aumenta a quantidade de pães de queijo necessários conforme a fase avança
        metaPaesDeQueijo = 2 + (faseAtual / 4);
        return tempoGlobalSugerido;
    }

    public void AddIngredient(IngredientData data)
    {
        if (jogoFinalizado || !jogoIniciado) return; // Trava de segurança

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
        if (jogoFinalizado) return; 

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
                    LimparMesa();
                    // exibe imagem dos pao de queijo feitos
                    StartCoroutine(RotinaVitoria());
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


private IEnumerator RotinaVitoria()
    {
        if (painelVitoriaFinal != null) painelVitoriaFinal.SetActive(true);
        if (confetesParticulas != null) confetesParticulas.Play();

        yield return new WaitForSeconds(tempoAtrasoVitoria);
        
        Vencer(); 
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;

        // Troca o visual do cozinheiro para triste quando o tempo acaba!
        if (cozinheiroSpriteRenderer != null && cozinheiroTristeSprite != null)
        {
            cozinheiroSpriteRenderer.sprite = cozinheiroTristeSprite;
        }

        base.TempoEsgotado();
    }
}