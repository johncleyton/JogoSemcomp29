using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectIngredients : MinigameBase
{
    [Tooltip("Put the ingrediens here")]
    public List<IngredientData> ingredients;

    public int currentQuantity;

    // para chamar em outro script
    public static CollectIngredients instance;

    public Transform mesaTransform;
    public GameObject iconPaoPrefab;
    public GameObject iconQueijoPrefab;

    [Header("Controle de Vitória")]
    public int metaPaesDeQueijo = 3;
    private int paesFeitos = 0;

    void Start()
    {
        instance = this;
        currentQuantity = ingredients.Count;
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
        if (jogoFinalizado) return; // Trava de segurança

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
                    Vencer(); // Notifica o GameManagerRework que o jogador bateu a meta
                    return;
                }
            }
            else if (i1 == "Pao" && i2 == "Pao")
            {
                Debug.Log("PÃO DE PÃO! Muito duro!"); 
            }
            else if (i1 == "Queijo" && i2 == "Queijo")
            {
                Debug.Log("Muito Mole! Derretido!"); 
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

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;
        base.TempoEsgotado();
    }
}