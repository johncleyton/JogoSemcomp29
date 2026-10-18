using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class JacareManager : MinigameBase
{
    double cooldown = 1.5;
    float timer_total = 0f;
    private float velocidadeAtual = 2f;

    [Header("Configurações de Spawn")]
    public GameObject jacarePrefab;
    public Transform[] spawnPoints;
    public Sprite[] spawnSprites;
    public Sprite[] clickedSprites;

    [Header("Configurações do Barco")]
    public SpriteRenderer barcoRenderer;
    public Sprite spriteVitoria;
    public Sprite spriteDerrota;

    private float timer = 0f;

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        float tempoFixo = 10f;

        cooldown = 1.5 - (faseAtual / 50);
        if (cooldown < 0.3f) cooldown = 0.3f;

        velocidadeAtual = 2f + (faseAtual * 0.2f);
        return tempoFixo;
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        if(barcoRenderer != null && spriteVitoria != null)
        {
            barcoRenderer.sprite = spriteVitoria;
        }
        Vencer();
    }
    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        if (jogoFinalizado) return;
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

        GameObject newJacare = Instantiate(jacarePrefab, chosenSpawn.position, chosenSpawn.rotation);
        if(spawnSprites != null && randomIndex < spawnSprites.Length && spawnSprites[randomIndex] != null)
        {
            SpriteRenderer renderer = newJacare.GetComponent<SpriteRenderer>();

            if (renderer != null)
            {
                renderer.sprite = spawnSprites[randomIndex];
            }
            JacareBehaviour behaviour = newJacare.GetComponent<JacareBehaviour>();
            if (behaviour != null && clickedSprites != null && randomIndex < clickedSprites.Length)
            {
                behaviour.velocidade = velocidadeAtual;
                behaviour.spriteOnClick = clickedSprites[randomIndex];
            }
        }
    }

    public void MudarBarcoDerrota()
    {
        if(barcoRenderer != null && spriteDerrota != null)
        {
            barcoRenderer.sprite = spriteDerrota;
        }

        JacareBehaviour[] jacaresVivos = FindObjectsOfType<JacareBehaviour>();

        foreach(JacareBehaviour jacare in jacaresVivos)
        {
            Destroy(jacare.gameObject);
        }
    }
}
