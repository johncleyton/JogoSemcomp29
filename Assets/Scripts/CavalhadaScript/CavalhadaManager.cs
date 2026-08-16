using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CavalhadaManager : MinigameBase
{
    public GameObject[] lanes;
    public GameObject cavalo;
    public float cooldown = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(summonCavalo), cooldown, cooldown);
    }

    void summonCavalo()
    {
        int random = Random.Range(0, lanes.Length);
        Instantiate(cavalo, lanes[random].transform.position, Quaternion.identity, lanes[random].transform);
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // O tempo que voces vao usar
        float tempoFixo = 10f; 
        return tempoFixo;
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) 
            return;
        Vencer();
    }
}
