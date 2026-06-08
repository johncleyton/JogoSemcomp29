using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValorCarta : MonoBehaviour
{
    public int valorCarta;
    void Start()
    {
        Debug.Log("Valor da carta: " + valorCarta);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setValor(int valor)
    {
        valorCarta = valor;
    }

    public int getValor()
    {
        return valorCarta;
    }
}
