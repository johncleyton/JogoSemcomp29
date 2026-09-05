using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MangaManager : MinigameBase
{
    double cooldown = 1.5;
    float timer_total = 0f;


    [Header("Configurações de Spawn")]
    public GameObject mangaPrefab;
    public Transform areaTop;

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
        if (mangaPrefab == null || areaTop == null) return;

        SpriteRenderer renderizerTop = areaTop.GetComponent<SpriteRenderer>();

        if(renderizerTop != null)
        {
            float minX = renderizerTop.bounds.min.x;
            float maxX = renderizerTop.bounds.max.x;

            float randomX = Random.Range(minX, maxX);

            Vector3 spawnPoint = new Vector3(randomX, areaTop.position.y, 0f);

            Instantiate(mangaPrefab, spawnPoint, Quaternion.identity);
        }
    }
}
