using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnIngredients : MonoBehaviour
{
    [Tooltip("Drag here Pao and Queijo prefabs")]
    public GameObject[] ingredientPrefabs;
    public Transform targetPoint;
    
    [Header("Configurações de Velocidade")]
    [Tooltip("Tempo entre cada lançamento. Deixei baixo (0.3s) para chover ingrediente!")]
    public float tempoEntreLancamentos = 0.3f;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        if (targetPoint == null) Debug.LogError("SpawnIngredients: 'targetPoint' faltando.");
        if (ingredientPrefabs == null || ingredientPrefabs.Length == 0) Debug.LogError("SpawnIngredients: prefab faltando.");
        
        // O InvokeRepeating foi removido daqui e passado para o método abaixo
    }

    // Função chamada pelo CollectIngredients assim que o jogador clicar
    public void ComecarSpawns()
    {
        InvokeRepeating(nameof(LaunchProjectile), 0.1f, tempoEntreLancamentos);
    }

    public void LaunchProjectile()
    {
        if (targetPoint == null || ingredientPrefabs == null || ingredientPrefabs.Length == 0) return;

        // Só lança se o jogo já começou e não estiver finalizado
        if (CollectIngredients.instance != null && CollectIngredients.instance.JogoEncerrado) return;

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
        int ladoSorteado = Random.Range(0, 3);
        Vector3 viewportPoint = Vector3.zero;
        float posicaoAleatoria = Random.Range(0f, 1f);

        switch (ladoSorteado)
        {
            case 0: viewportPoint = new Vector3(0f, posicaoAleatoria, 0f); break;
            case 1: viewportPoint = new Vector3(posicaoAleatoria, 1f, 0f); break;
            case 2: viewportPoint = new Vector3(1f, posicaoAleatoria, 0f); break;
        }

        viewportPoint.z = Mathf.Abs(cam.transform.position.z);
        Vector3 worldPos = cam.ViewportToWorldPoint(viewportPoint);

        return new Vector2(worldPos.x, worldPos.y);
    }
}