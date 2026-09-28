using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarretaManager : MinigameBase
{

    public Transform pontoDeParada;

    public float velocidade = 5f;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, pontoDeParada.position, velocidade*Time.deltaTime);
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        float tempoFixo = 15f;
        return tempoFixo;
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        Vencer();
    }
}
