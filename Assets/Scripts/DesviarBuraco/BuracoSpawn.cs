using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BuracoSpawn : MonoBehaviour
{
    public GameObject prefabBuraco; 
    
    public float laneDistance = 6f; 
    
    public float spawnY = 3f; 

    [Header("Dificuldade")]
    public float spawnTime = .8f; 
    public float buracoSpeed = 10f;  

    private float currentSpawnTime;
    private float currentBuracoSpeed;

    private int lastLane = -1;

    private float timer;

    void Start()
    {
        int level = GameManagerRework.Instance != null ? GameManagerRework.Instance.faseAtual : 1;
        int dificulty = level / 5 ;

        currentSpawnTime = Mathf.Max(0.4f, spawnTime - (dificulty * 0.3f));
        currentBuracoSpeed = buracoSpeed + (dificulty * 2.5f);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= currentSpawnTime)
        {
            SpawnBuraco();
            timer = 0f;
        }
    }

    void SpawnBuraco()
    {
        int lane;
        //nao deixando spawnar na mesma faixa
        do
        {
            lane = Random.Range(0, 3);
        } 
        while (lane == lastLane);

        lastLane = lane;


        float x = (lane - 1) * laneDistance - laneDistance;
        Vector3 spawnPosition = new Vector3(x, spawnY, 0f);

        GameObject buraco = Instantiate(prefabBuraco, spawnPosition, Quaternion.identity);

        //mudando a velocidade
        Buraco scriptBuraco = buraco.GetComponent<Buraco>();
        if (scriptBuraco != null)
        {
            scriptBuraco.speed = currentBuracoSpeed;
        }
    }
}
