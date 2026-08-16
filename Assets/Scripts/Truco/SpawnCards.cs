using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCards : MonoBehaviour
{
    int qtasCartas;
    public GameObject prefabCarta;
    // Start is called before the first frame update
    void Start()
    {
        qtasCartas = 7;
        int qualCarta = Random.Range(0, qtasCartas);
        for (int i = 0; i < qtasCartas; i++)
        {
            GameObject instanciaCarta = Instantiate(prefabCarta, gameObject.transform);
            //bool random = Random.value < 0.3f;
            //Debug.Log("Valor do random: " + random);
            instanciaCarta.GetComponent<ScriptCarta>().ehZap = (qualCarta == i) ? true : false;
        }
    }

}
