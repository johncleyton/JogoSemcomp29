using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGalhos : MonoBehaviour
{
    public GameObject prefab;

    [Header("Spawn")]
    public float baseSpawnTimer = 2f;
    private float spawnTimerAtual;

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

        spawnTimerAtual = baseSpawnTimer; 
    }

    private void Update()
    {
        if (!canSpawn) return;

        Instantiate(prefab, spawnPoints[Random.Range(0, spawnPoints.Length)]);
        StartCoroutine(BlockSpawn());
    }

    private IEnumerator BlockSpawn()
    {
        canSpawn = false;
        yield return new WaitForSeconds(spawnTimerAtual);
        canSpawn = true;
    }

    public void SetSpawnTimer(float novoTimer)
    {
        spawnTimerAtual = Mathf.Max(0.1f, novoTimer);
    }
}