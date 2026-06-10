using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NovoIngrediente", menuName = "Jogo/Ingrediente")]
public class IngredientData : ScriptableObject
{
    public string nameIngredient;
    public Sprite iconUI;

    public GameObject prefabModel;
}
