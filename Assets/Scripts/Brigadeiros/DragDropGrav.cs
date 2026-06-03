using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDropGrav : MonoBehaviour
{
    private bool isClicked = false;
    Rigidbody2D rb;
<<<<<<< HEAD
    List<Vector3> ponto_momento = new List<Vector3>();

    private void Awake()
    {
        ponto_momento.Add(Vector3.zero);
        ponto_momento.Add(Vector3.zero);
=======

    private void Awake()
    {
>>>>>>> MergePt2
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
<<<<<<< HEAD
        //Transforma os valores dos eixos do mouse na tela do computador para a tela do jogo
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //por algum motivo ta em 3D, entao precisa de um z pros brigadeiros ficarem na frente de tudo
        mouseWorldPos.z = -4.92f;

        //Pega os pontos do frame atual e do frame passado
        ponto_momento[0] = ponto_momento[1];
        ponto_momento[1] = mouseWorldPos;
        //Debug.Log(vetor_momento[0]);
        //Debug.Log(vetor_momento[1]);

        if (isClicked)
        {
            //Debug.Log(mouseWorldPos);
            //O que faz o brigadeiro mover
            transform.position = mouseWorldPos;
        }

=======
        if (isClicked)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = -4.92f;
            //Debug.Log(mouseWorldPos);
            transform.position = mouseWorldPos;

        }
>>>>>>> MergePt2
    }

    private void OnMouseDown()
    {
        isClicked = true;
<<<<<<< HEAD

        //nao lembro porque deixar gravidade 0
        //rb.gravityScale = 0f;
=======
        rb.gravityScale = 0f;
>>>>>>> MergePt2
    }

    private void OnMouseUp()
    {
<<<<<<< HEAD
        //rb.gravityScale = 1f;

        isClicked = false;

        //Tira o momento
        rb.velocity = Vector3.zero;


        //Debug.Log(vetor_momento[1] - vetor_momento[0]);
        //Tira um vetor a partir dos pontos do brigadeiro nos dois
        //ultimos frames e adiciona uma forca nessa direcao
        rb.AddForce(120 * (ponto_momento[1] - ponto_momento[0]), ForceMode2D.Impulse);
=======
        isClicked = false;
        rb.gravityScale = 1f;
        rb.velocity = Vector3.zero;
>>>>>>> MergePt2
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
<<<<<<< HEAD
        //Layer 4 eh "mouth"
        if (collision.gameObject.layer == 4)
        {
            Destroy(gameObject);
            GameManager.qtdBrigadeiro -= 1;
=======
        if (collision.gameObject.layer == 4)
        {
            Destroy(gameObject);
>>>>>>> MergePt2
        }
    }
}
