using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GramaMove : MonoBehaviour
{
    public float velocidade = 5f;
    public float limiteInferior = -10f;
    public float pontoDeReset = 10f;  

    void Update()
    {
        transform.Translate(Vector3.down * velocidade * Time.deltaTime);

        if (transform.position.y <= limiteInferior)
        {
            Vector3 novaPosicao = transform.position;
            novaPosicao.y = pontoDeReset;
            transform.position = novaPosicao;
        }
    }
}
