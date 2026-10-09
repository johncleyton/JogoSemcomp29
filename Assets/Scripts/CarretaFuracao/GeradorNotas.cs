using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeradorNotas : MonoBehaviour
{

    public GameObject notaPrefab;

    public Transform pontoSpawn;

    public float intervaloSpawn = 1f;

    public int notasGeradas = 0;

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
                FindObjectOfType<MinigameBase>().Perder();
            }

        }
    }

    IEnumerator TemporizadorDeNotas()
    {
        while (true)
        {
            float tempoAtual = intervaloSpawn - notasGeradas*0.2f;
            yield return new WaitForSeconds(tempoAtual < 0.5f ? 0.5f : tempoAtual);
            GameObject novaNota = Instantiate(notaPrefab, pontoSpawn.position, Quaternion.identity);
            notasGeradas += 1;
            notasAtivas.Enqueue(novaNota.GetComponent<NotaManager>());
        }
    }
}
