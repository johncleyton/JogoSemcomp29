using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EstadoOvelha
{
    Nova,
    Perfeita,
    Velha
}


public class OvelhaController : MonoBehaviour
{


    public EstadoOvelha estadoAtual;


    public SpriteRenderer spriteRenderer;

    // array de sprites -> sortear o estado atual


    public void Inicializar(EstadoOvelha estado)
    {
        estadoAtual = estado;
        // config visuals
    }

    public void SairDaTela()
    {

        // indo embora -> animator ou dotween
        Destroy(gameObject, 1f);
    }
}
