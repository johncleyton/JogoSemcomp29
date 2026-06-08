using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truco : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] spawnpoints;
    public GameObject carta;
    void Start()
    {
        for (int i = 0; i < spawnpoints.Length; i++)
        {
            GameObject cartaIntancia = Instantiate(carta, spawnpoints[i].transform.position, Quaternion.identity);
            cartaIntancia.GetComponent<ValorCarta>().setValor(Random.Range(0, 10));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
