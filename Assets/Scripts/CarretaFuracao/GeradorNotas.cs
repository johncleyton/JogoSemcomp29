using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeradorNotas : MonoBehaviour
{

    public GameObject notaPrefab;

    public Transform pontoSpawn;

    public float intervaloSpawn = 1f;

    public int notasGeradas = 0;

    public Sprite spriteNota1;

    public Sprite spriteNota2;

    private bool flagSpriteNota = true;

    Queue<NotaManager> notasAtivas = new Queue<NotaManager>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TemporizadorDeNotas());
    }

    // Update is called once per frame
    void Update()
    {
        //Adicao de "!PauseBase.mouseOver" para resolver bug de pause
        //Se o jogador clicar no botao de pause, nenhuma acao eh feita
        if (Input.GetMouseButtonDown(0) && notasAtivas.Count > 0 && !PauseBase.mouseOver)
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
            novaNota.GetComponent<SpriteRenderer>().sprite = flagSpriteNota ? spriteNota1 : spriteNota2;
            flagSpriteNota = !flagSpriteNota;
            notasGeradas += 1;
            notasAtivas.Enqueue(novaNota.GetComponent<NotaManager>());
        }
    }
}
