using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class JacareManager : MinigameBase
{
    double cooldown = 1.5;
    float timer_total = 0f;

    [Header("Configurações de Spawn")]
    public GameObject jacarePrefab;
    public Transform[] spawnPoints;

    private float timer = 0f;

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        float tempoFixo = 10f;

        cooldown -= faseAtual / 50;
        return tempoFixo;
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        Vencer();
    }
    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        timer_total += Time.deltaTime;
 
        timer += Time.deltaTime;

        if (timer >= (float)cooldown)
        {
            SpawnObject();
            timer = 0f;
        }

        if (timer_total >= 10f)
        {
            TempoEsgotado();
        }

    }

    void SpawnObject()
    {
        if (spawnPoints.Length == 0 || jacarePrefab == null)
        {
            Debug.LogWarning("Prefab ou Spawnpoints não configurados no Game Manager");
            return;
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawn = spawnPoints[randomIndex];
        Instantiate(jacarePrefab, chosenSpawn.position, chosenSpawn.rotation);
    }
}
