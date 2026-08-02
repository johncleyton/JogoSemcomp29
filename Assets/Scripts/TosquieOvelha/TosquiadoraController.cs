using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TosquiadoraController : MonoBehaviour
{

    public float velocidadeHorizontal = 5f;

    public float limiteEsq = -2.5f;

    public float limiteDir = 2.5f;

    public float velocidadeCorte = 15f;
    public float distanciaCorte = 3f;

    private Vector3 posicaoBase;
    private bool cortando = false;
    private bool regressando = false;
    private int direcao = 1; // 1 para dir e -1 para esq

    // Start is called before the first frame update
    void Start()
    {
        posicaoBase = transform.position;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!cortando && !regressando)
        {
            MoverHorizontal();

            if (Input.GetMouseButtonDown(0))
            {
                cortando = true;
            }
        }

        else if (cortando)
        {
            DescerCorte();
        }else if (regressando)
        {
            SubirBase();
        }
    }


    private void MoverHorizontal()
    {
        transform.Translate(Vector3.right * direcao * velocidadeHorizontal * Time.deltaTime);


        if(transform.position.x >= limiteDir)
        {
            direcao = -1;
        }

        else if(transform.position.x <= limiteEsq)
        {
            direcao = 1;
        }

        posicaoBase = new Vector3(transform.position.x, posicaoBase.y, transform.position.z);
    }


    private void DescerCorte()
    {
        // Dispara a tosquiadora para baixo
        transform.position += Vector3.down * velocidadeCorte * Time.deltaTime;

        // Se atingiu a distância máxima de corte, começa a regressar
        if (transform.position.y <= posicaoBase.y - distanciaCorte)
        {
            cortando = false;
            regressando = true;
        }
    }

    private void SubirBase()
    {
        // Puxa a tosquiadora de volta para cima
        transform.position += Vector3.up * velocidadeCorte * Time.deltaTime;

        // Se voltou à altura original, pode mover-se lateralmente outra vez
        if (transform.position.y >= posicaoBase.y)
        {
            // Garante que fica exatamente na linha base
            transform.position = new Vector3(transform.position.x, posicaoBase.y, transform.position.z);
            regressando = false;
        }
    }
}
