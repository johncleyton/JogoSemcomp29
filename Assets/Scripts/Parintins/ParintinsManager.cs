using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ParintinsManager : MinigameBase
{

    double cooldown = 0.5;
    float timer_total = 0f;


    [Header("Configurações de Spawn")]
    public GameObject prefabCoracao;
    public GameObject prefabEstrela;
    public Transform[] spawnpoints;

    [Header("Referências dos Bois")]
    public GameObject boiGarantido;
    public GameObject boiCaprichoso;

    private int boiEscolhido;
    private bool start = false;

    private float timer = 0f;
    private int counter = 0; 

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        float tempoFixo = 10f;

        cooldown -= faseAtual / 50;
        return tempoFixo;
    }

    
    // Start is called before the first frame update
    void Start()
    {

        boiEscolhido = UnityEngine.Random.value > 0.5f ? 1 : 0;

        StartCoroutine(ChooseBoiEscolhido());
    }

    IEnumerator ChooseBoiEscolhido()
    {
        GameObject chosen = boiEscolhido == 0 ? boiCaprichoso : boiGarantido;
        GameObject notChosen = boiEscolhido == 0 ? boiGarantido : boiCaprichoso;

        notChosen.SetActive(false);

        for (int i = 0; i < 3; i++)
        {
            chosen.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            chosen.SetActive(true);
            yield return new WaitForSeconds(0.2f);
        }

        notChosen.SetActive(true);
        start = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (!start) return; 
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

        if (counter == 5)
        {
            Vencer();
        }
    }

    void SpawnObject()
    {
        if (spawnpoints.Length == 0) return;

        List<Transform> emptySpawns = new List<Transform>();
        foreach(Transform point in spawnpoints)
        {
            if(point.childCount == 0)
            {
                emptySpawns.Add(point);
            }
        }

        if (emptySpawns.Count == 0) return;

        int indexSpawn = UnityEngine.Random.Range(0, emptySpawns.Count);
        Transform chosenSpawn = spawnpoints[indexSpawn];

        GameObject spawningPrefab = UnityEngine.Random.value > 0.5f ? prefabCoracao : prefabEstrela;

        Instantiate(spawningPrefab, chosenSpawn.position, Quaternion.identity, chosenSpawn);
    }

    public void Clicked(int typeOfItem)
    {
        if(typeOfItem == boiEscolhido)
        {
            counter++;
        }
        else
        {
            counter--;
        }
    }
}
