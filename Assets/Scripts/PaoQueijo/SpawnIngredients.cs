using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnIngredients : MonoBehaviour
{

    [Tooltip("Drag here Pao and Queijo prefabs")]
    public GameObject[] ingredientPrefabs;

    public Transform targetPoint;

    private Camera cam;

    void Awake()
    {
        // CORRIGIDO: evita chamar Camera.main repetidamente em CalculateSpawn
        cam = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        // CORRIGIDO: avisa claramente no Console se faltar configuração no
        // Inspector, em vez de deixar o InvokeRepeating lançar erro a cada 2s
        if (targetPoint == null)
        {
            Debug.LogError("SpawnIngredients: 'targetPoint' não foi definido no Inspector. Os ingredientes não terão para onde voar.");
        }

        if (ingredientPrefabs == null || ingredientPrefabs.Length == 0)
        {
            Debug.LogError("SpawnIngredients: nenhum prefab foi colocado em 'ingredientPrefabs'.");
        }

        // usado para calcular uma linha justa 
        // para tacar coisas no player
        // CalculateSpawn();

        // wait 2 sec, launch every 0.3 s 
        InvokeRepeating(nameof(LaunchProjectile), 2.0f, 2.0f);
    }


    public void LaunchProjectile()
    {
        // CORRIGIDO: aborta com segurança em vez de quebrar com exceção
        // se a configuração no Inspector estiver incompleta
        if (targetPoint == null || ingredientPrefabs == null || ingredientPrefabs.Length == 0)
        {
            return;
        }

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

        viewportPoint.z = Mathf.Abs(cam.transform.position.z);

        Vector3 worldPos = cam.ViewportToWorldPoint(viewportPoint);

        return new Vector2(worldPos.x, worldPos.y);
    }
}
