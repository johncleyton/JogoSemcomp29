using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaixaMove : MonoBehaviour
{
    public float limiteInferior = -32.27f;
    public float pontoDeReset = 32.27f;

    void Update()
    {
        if (BuracoSpawn.pararMovimento) return;

        float velocidade = BuracoSpawn.velocidadeAtual;

        transform.Translate(
            Vector3.down * velocidade * Time.deltaTime,
            Space.World
        );

        if (transform.position.y <= limiteInferior)
        {
            Vector3 novaPosicao = transform.position;
            novaPosicao.y = pontoDeReset;
            transform.position = novaPosicao;
        }
    }
}
