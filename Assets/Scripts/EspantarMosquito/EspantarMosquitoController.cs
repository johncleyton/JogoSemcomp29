using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EspantarMosquitos : MinigameBase
{
    public GameObject mosquitoPrefab;
    public Transform centerTransform; 
    public float spawnRadius = 16f;   

    public int baseMosquitoCount = 4;

    void Start()
    {
        int level = GameManagerRework.Instance != null ? GameManagerRework.Instance.faseAtual : 1;
        int difficulty = level / 5;

        int totalMosquitos = baseMosquitoCount + (difficulty * 2);

        SpawnMosquitos(totalMosquitos);
    }

    void SpawnMosquitos(int quantidade)
    {
        for (int i = 0; i < quantidade; i++)
        {
            float angle = Random.Range(0f, Mathf.PI);
            Vector3 spawnPos = centerTransform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnRadius;

            GameObject mosquitoObj = Instantiate(mosquitoPrefab, spawnPos, Quaternion.identity);
            Mosquito mosquitoScript = mosquitoObj.GetComponent<Mosquito>();
            
            if (mosquitoScript != null)
            {
                mosquitoScript.Initialize(centerTransform);
            }
        }
    }

    public void PerdeuJogo()
    {
        Perder(); 
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;
                jogoFinalizado = true;
        Vencer();
    }
}
