using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Espetinho : MinigameBase
{

    [SerializeField] GameObject comida;
    private float distVert;
    private float espTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        //determina de quanto em quanto tempo uma nova comida sera instanciada
        //quando tempoatual eh 3, o tempo para instanciar eh 0.25 sec
        //quando tempoatual eh 7, o tempo para instanciar eh 1.5874 sec
        distVert = 1/(Mathf.Pow(1.5874f,(6-GameManagerRework.Instance.tempoDoMinigameAtual)));

        //Debug.Log("tempo para nascer comida: "+distVert);
    }

    // Update is called once per frame
    void Update()
    {
        espTimer += Time.deltaTime;

        if (espTimer >= distVert)
        {
            //Debug.Log("INSTANCIOU A COMIDA");
            espTimer = 0f;

            //existem 6 posicoes possiveis para a comida nascer. As posicoes sao diferentes
            //apenas no eixo x. 
            Vector3 posicao = new Vector3((float)(-8 + 3.2 * Random.Range(0, 5)), 7f, -6f);
            Instantiate(comida, posicao, Quaternion.identity);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //o gameObject (EspetoManager) tem um boxcollider que fica embaixo da camera 
        //e detecta quando o jogador "deixa cair a comida"
        //layer 7 eh a layer do prefab da comida
        if (collision.gameObject.layer == 7)
        {
            Perder();
        }
    }
}
