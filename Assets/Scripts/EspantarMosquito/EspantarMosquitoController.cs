using System;
using UnityEngine;


public class EspantarMosquitos : MinigameBase
{
    public SpriteRenderer playerSpriteRenderer; 
    public Sprite spriteVitoria;                
    public Sprite spriteDerrota;

    public GameObject mosquitoPrefab;
    public Transform centerTransform; 
    public float spawnRadius = 8f;   

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
            Debug.Log("SPWANEI");
            float angle = UnityEngine.Random.Range(0f, Mathf.PI);
            Vector3 spawnPos = centerTransform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), -1f) * spawnRadius;

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
        playerSpriteRenderer.sprite = spriteDerrota;
        DestruirMosquitos();
        PerderComAtraso(2f); 
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;
        playerSpriteRenderer.sprite = spriteVitoria;
        DestruirMosquitos();
        VencerComAtraso(2f);
    }

    private void DestruirMosquitos()
    {
        Mosquito[] mosquitos = FindObjectsByType<Mosquito>(FindObjectsSortMode.None);

        foreach (Mosquito m in mosquitos)
        {
            Destroy(m.gameObject);
        }
    }
}
