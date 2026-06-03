using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnButtons : MonoBehaviour
{
    public GameObject btn;
    public GameObject[] spawnPoints;
    public int qtde;
    List<int> numerosAleatorios = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < qtde; i++)
        {
            int random = 0;
            while (numerosAleatorios.Contains(random))
                random = Random.Range(0, spawnPoints.Length);
            numerosAleatorios.Add(random);
            Debug.Log(random);
            Instantiate(btn, spawnPoints[random].transform.position, Quaternion.identity, spawnPoints[random].transform);
        }
    }

}
