using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeradorNotas : MinigameBase
{

    public GameObject notaPrefab;

    public Transform pontoSpawn;

    public float intervaloSpawn = 1f;

    Queue<NotaManager> notasAtivas = new Queue<NotaManager>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TemporizadorDeNotas());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && notasAtivas.Count > 0)
        {
            NotaManager frente = notasAtivas.Peek();
            if (frente.estaNaZonaDeAcerto)
            {
                notasAtivas.Dequeue();
                frente.Acertar();
            } else
            {
                Perder();
            }

        }
    }

    IEnumerator TemporizadorDeNotas()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloSpawn);
            GameObject novaNota = Instantiate(notaPrefab, pontoSpawn.position, Quaternion.identity);
            notasAtivas.Enqueue(novaNota.GetComponent<NotaManager>());
        }
    }
}
