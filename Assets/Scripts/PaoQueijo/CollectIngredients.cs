using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectIngredients : MonoBehaviour
{

    [Tooltip("Put the ingrediens here")]
    public List<IngredientData> ingredients;

    public int currentQuantity;

    // para chamar em outro script
    public static CollectIngredients instance;

    public Transform mesaTransform;
    public GameObject iconPaoPrefab;
    public GameObject iconQueijoPrefab;

    void Start()
    {
        instance = this;

        currentQuantity = ingredients.Count;
    }

    void Update()
    {
        // mostrar o "inventario" na mesa;

        // fazer tuplas de objetos, PAO + QUEIJO

    }


    public void AddIngredient(IngredientData data)
    {
        ingredients.Add(data);
        currentQuantity = ingredients.Count; // CORRIGIDO: antes ficava parado desde o Start()

        GameObject novoIcon = null;

        if (data.nameIngredient == "Pao") novoIcon = iconPaoPrefab;
        else if (data.nameIngredient == "Queijo") novoIcon = iconQueijoPrefab;

        if (novoIcon != null && mesaTransform != null)
        {
            Instantiate(novoIcon, mesaTransform);
        }

        // CORRIGIDO: antes era preciso lembrar de chamar VerifyRevenue() manualmente
        // de fora (em CursorCustomPao) toda vez depois de AddIngredient(). Agora a
        // própria classe garante que a verificação acontece assim que a mesa enche,
        // não importa de onde AddIngredient() seja chamado.
        VerifyRevenue();
    }


    public void VerifyRevenue()
    {
        if (ingredients.Count == 2)
        {
            string i1 = ingredients[0].nameIngredient;
            string i2 = ingredients[1].nameIngredient;

            if ((i1 == "Pao" && i2 == "Queijo") || (i1 == "Queijo" && i2 == "Pao"))
            {
                Debug.Log("Pão de Queijo Perfeito!");
                // Adiciona pontos
            }
            else if (i1 == "Pao" && i2 == "Pao")
            {
                Debug.Log("PÃO DE PÃO! Muito duro!"); // Reação de muito duro
            }
            else if (i1 == "Queijo" && i2 == "Queijo")
            {
                Debug.Log("Muito Mole! Derretido!"); // Reação de muito mole
            }

            ingredients.Clear(); // Limpa a mesa para a próxima rodada
            currentQuantity = ingredients.Count; // CORRIGIDO: sincroniza depois de limpar

            // CORRIGIDO: evita NullReferenceException se mesaTransform não tiver
            // sido arrastado no Inspector
            if (mesaTransform != null)
            {
                foreach (Transform child in mesaTransform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

}
