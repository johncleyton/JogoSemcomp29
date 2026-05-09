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

}
