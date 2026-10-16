using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnIngredients : MonoBehaviour
{
    [Tooltip("Drag here Pao and Queijo prefabs")]
    public GameObject[] ingredientPrefabs;
    public Transform targetPoint;

    [Header("Configurações de Velocidade")]
    [Tooltip("Tempo entre cada lançamento.")]
    public float tempoEntreLancamentos = 0.3f;

    [Tooltip("Quantos ingredientes saem de uma vez a cada lançamento. Aumente para encher mais a tela.")]
    public int ingredientesPorLancamento = 2;

    private Camera cam;

    // "Sacola" embaralhada: cada rodada contém UM de cada prefab, em ordem aleatória.
    // Assim nunca vêm muitos ingredientes iguais em sequência (no máximo 2 do mesmo tipo).
    private readonly List<int> sacola = new List<int>();

    void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        if (targetPoint == null) Debug.LogError("SpawnIngredients: 'targetPoint' faltando.");
        if (ingredientPrefabs == null || ingredientPrefabs.Length == 0) Debug.LogError("SpawnIngredients: prefab faltando.");
    }

    // Função chamada pelo CollectIngredients assim que o jogador clicar
    public void ComecarSpawns()
    {
        CancelInvoke(nameof(LaunchProjectile)); // evita spawn duplicado se for chamada 2x
        InvokeRepeating(nameof(LaunchProjectile), 0.1f, tempoEntreLancamentos);
    }

    // Chamada pelo CollectIngredients na vitória / fim de tempo
    public void PararSpawns()
    {
        CancelInvoke(nameof(LaunchProjectile));
    }

    public void LaunchProjectile()
    {
        if (targetPoint == null || ingredientPrefabs == null || ingredientPrefabs.Length == 0) return;

        // Só lança se o jogo não estiver encerrado
        if (CollectIngredients.instance != null && CollectIngredients.instance.JogoEncerrado) return;

        for (int i = 0; i < ingredientesPorLancamento; i++)
        {
            LancarUm();
        }
    }

    void LancarUm()
    {
        GameObject prefabSorted = ingredientPrefabs[ProximoIndice()];
        Vector2 spawnPos = CalculateSpawn();

        GameObject newIngredient = Instantiate(prefabSorted, spawnPos, Quaternion.identity);

        IngredientBehaviour ingredientBehaviour = newIngredient.GetComponent<IngredientBehaviour>();
        if (ingredientBehaviour)
        {
            ingredientBehaviour.targetPos = targetPoint.position;
        }
    }

    int ProximoIndice()
    {
        if (sacola.Count == 0)
        {
            for (int i = 0; i < ingredientPrefabs.Length; i++) sacola.Add(i);

            // Embaralha (Fisher-Yates)
            for (int i = sacola.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = sacola[i];
                sacola[i] = sacola[j];
                sacola[j] = tmp;
            }
        }

        int ultimo = sacola.Count - 1;
        int indice = sacola[ultimo];
        sacola.RemoveAt(ultimo);
        return indice;
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