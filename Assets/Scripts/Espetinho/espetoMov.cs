using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class espetoMov : MinigameBase
{
    //esse codigo ta atrelhado a um objeto invisivel (collider_movimento) para poder
    //detectar clique do jogador no espetinho em uma area maior do
    //que o collider do proprio espetinho

    private bool isClicked = false;

    [SerializeField] GameObject espetinho;

    private Transform[] comidas;
    private int contagem = 0;

    //[SerializeField] Rigidbody2D espRb;
    [SerializeField] Collider2D espCol;
    [SerializeField] Collider2D esseCol;
    //Rigidbody2D rb;

    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        Vencer();
    }
    void Start()
    {
        comidas = new Transform[6];

        //rb = GetComponent<Rigidbody2D>();
        //rb.freezeRotation = true;
        //rb.constraints = rb.constraints | RigidbodyConstraints2D.FreezePositionY;

        //espRb.freezeRotation = true;
        //espRb.constraints = espRb.constraints | RigidbodyConstraints2D.FreezePositionY;

        //ignora colisao entre o collider do espetinho e o collider que detecta quando uma
        //comida foi espetada
        Physics2D.IgnoreCollision(espCol,esseCol);
    }

    // Update is called once per frame
    void Update()
    {
        //rb.velocity = Vector3.zero;
        //espRb.velocity = Vector3.zero;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mouseWorldPos.z = -5f;
        mouseWorldPos.y = -3.76f;

        if (isClicked)
        {
            //move tanto o espetinho quanto o collider_movimento para 
            //onde o mouse/dedo esta na tela quando clicado
            espetinho.transform.position = mouseWorldPos;
            transform.position = mouseWorldPos;
            //Debug.Log(mouseWorldPos);

            //aqui eh sobre as posicoes das comidas depois de espetadas
            //cada vez que uma comida nova eh espetada, todas as outras
            //vao uma posicao para baixo.
            //Existem 5 posicoes possiveis (sendo a ultima quase invisivel
            //para o jogador). Vao de -2.4f ate -5.2f com distancia de -0.7f
            mouseWorldPos.y = -1.7f;
            for (int i = 5; i > -1; i--)
            {
                if (comidas[i] != null)
                {
                    mouseWorldPos.y -= 0.7f;
                    if (mouseWorldPos.y < -5.2f)
                    {
                        //o i aqui eh sempre 0 e so acontece quando tem
                        //6 Transforms no vetor
                        Destroy(comidas[i].gameObject);
                        for (int j = 0; j < 5; j++)
                        {
                            comidas[j] = comidas[j + 1];
                        }
                        //deixar o vetor com 5 Transforms. Se nao, tudo desaparece!
                        comidas[5] = null;
                    }
                    else
                    {
                        //move as comidas para onde o mouse/dedo esta quando clicado
                        comidas[i].transform.position = mouseWorldPos;
                    }
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //layer 7 eh a layer do prefab da comida
        if (collision.gameObject.layer == 7)
        {
            //Debug.Log("COLIDIU");
            
            comidas[contagem] = collision.gameObject.GetComponent<Transform>();

            collision.gameObject.GetComponent<Collider2D>().enabled = false;
            collision.gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
            collision.gameObject.GetComponent<Rigidbody2D>().constraints = collision.gameObject.GetComponent<Rigidbody2D>().constraints | RigidbodyConstraints2D.FreezePosition;

            if (contagem < 5)
            {
                contagem++;
            }
        }
    }

    private void OnMouseDown()
    {
        isClicked = true;
        //Debug.Log("CLICK");
    }
    private void OnMouseUp() 
    { 
        isClicked = false;
        //Debug.Log("DESBLICK");
    }

}
