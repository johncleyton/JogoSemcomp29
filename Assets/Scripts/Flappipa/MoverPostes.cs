using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverPostes : MonoBehaviour
{

    public float velocidade = 5f;
    public float limiteEsquerdo = -30f;
    
    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.left * velocidade * Time.deltaTime;

        if (transform.position.x < limiteEsquerdo)
        {
            Destroy(gameObject);
        }

    }
}
