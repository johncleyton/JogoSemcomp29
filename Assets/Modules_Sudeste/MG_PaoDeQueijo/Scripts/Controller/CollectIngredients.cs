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
    }


    public void VerifyRevenue()
    {
        if(ingredients.Count == 2)
        {
            string i1 = ingredients[0].nameIngredient;
            string i2 = ingredients[1].nameIngredient;

            if ((i1 == "Pao" && i2 == "Queijo") || (i1 == "Queijo" && i2 == "Pao")) {
            Debug.Log("Pão de Queijo Perfeito!");
            // Adiciona pontos
            } else if (i1 == "Pao" && i2 == "Pao") {
                Debug.Log("PÃO DE PÃO! Muito duro!"); // Reação de muito duro [cite: 69]
            } else if (i1 == "Queijo" && i2 == "Queijo") {
                Debug.Log("Muito Mole! Derretido!"); // Reação de muito mole [cite: 69]
            }
            ingredients.Clear(); // Limpa a mesa para a próxima rodada
        }
    }

}
