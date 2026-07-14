using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NovoIngrediente", menuName = "Jogo/Ingrediente")]
public class IngredientData : ScriptableObject
{
    public string nameIngredient;
    public Sprite iconUI;

    // OBSERVAÇÃO: hoje quem é efetivamente instanciado ao nascer um
    // ingrediente é o array "ingredientPrefabs" de SpawnIngredients, não
    // este campo. Mantido sem alteração para uso futuro (ex: exibir o
    // modelo em outro lugar da UI).
    public GameObject prefabModel;
}
