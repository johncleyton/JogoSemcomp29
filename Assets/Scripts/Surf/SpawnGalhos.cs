using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGalhos : MonoBehaviour
{
    public GameObject prefab;
    public GameManager gameManager;

    [Header("Spawn")]
    public float baseSpawnTimer = 2f;

    private bool canSpawn = true;
    private Transform[] spawnPoints;

    void Awake()
    {
        spawnPoints = new Transform[transform.childCount];
        int i = 0;
        foreach (Transform t in transform)
        {
            spawnPoints[i] = t;
            i++;
        }
    }

    private void Update()
    {
        if (!canSpawn) return;
        Instantiate(prefab, spawnPoints[Random.Range(0, spawnPoints.Length)]);
        StartCoroutine(BlockSpawn());
    }

    private float GetCurrentSpawnTimer()
    {
        float proporcao = GlobalVariables.timer / GlobalVariables.timerInicial;
        return baseSpawnTimer * proporcao;
    }

    private IEnumerator BlockSpawn()
    {
        canSpawn = false;
        yield return new WaitForSeconds(GetCurrentSpawnTimer());
        canSpawn = true;
    }
}