using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnIngredients : MonoBehaviour
{

    [Tooltip("Drag here Pao and Queijo prefabs")]
    public GameObject[] ingredientPrefabs;

    public Transform targetPoint; 
    // Start is called before the first frame update
    void Start()
    {
        // usado para calcular uma linha justa 
        // para tacar coisas no player
        // CalculateSpawn();

        // wait 2 sec, launch every 0.3 s 
        InvokeRepeating(nameof(LaunchProjectile), 2.0f, 2.0f);
    }


    public void LaunchProjectile()
    {
        int randomIndex = Random.Range(0, ingredientPrefabs.Length);

        GameObject prefabSorted = ingredientPrefabs[randomIndex];


        Vector2 spawnPos = CalculateSpawn();

        GameObject newIngredient = Instantiate(prefabSorted, spawnPos, Quaternion.identity);


        IngredientBehaviour ingredientBehaviour = newIngredient.GetComponent<IngredientBehaviour>();

        if (ingredientBehaviour)
        {
            ingredientBehaviour.targetPos = targetPoint.position;
        } 
    }


    Vector2 CalculateSpawn()
    {

        // A ideia é, de algum canto, acima da mesa e do personagem, 
        // vai ser TACADO no personagem um ingrediente, 


        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        // esq = 0, topo = 1, dir = 2
        int ladoSorteado = Random.Range(0, 3);
        
        Vector3 viewportPoint = Vector3.zero;

        float posicaoAleatoria = Random.Range(0f, 1f);

        switch (ladoSorteado)
        {
            case 0: // esq, (x = 0, Y aleatorio)
                viewportPoint = new Vector3(0f, posicaoAleatoria, 0f);
                break;

            case 1: // Topo (X = aleatório, Y = 1)
                viewportPoint = new Vector3(posicaoAleatoria, 1f, 0f);
                break;
            case 2: // Direita (X = 1, Y = aleatório)
                viewportPoint = new Vector3(1f, posicaoAleatoria, 0f);
                break;
        }


        // viewport: z distancia da camera para o background

        viewportPoint.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewportPoint);

        return new Vector2(worldPos.x, worldPos.y);
    }
}
