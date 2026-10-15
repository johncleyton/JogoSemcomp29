using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverNuvem : MonoBehaviour
{

    public float velocidade = 2.5f;
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
