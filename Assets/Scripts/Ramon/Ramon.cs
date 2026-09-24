using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ramon : MinigameBase
{
    private Animator anim;
    private bool fazendoBarra = false;
    private bool naofezBarra = false;
    private int qtd = Mathf.RoundToInt((100/Mathf.Pow(1.38f, GameManagerRework.Instance.tempoDoMinigameAtual)));
    public TextMeshProUGUI barra;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        //Quando timer=7, qtdBarras eh 10, quando timer=3, qtdBarras eh 38
        barra.text = qtd.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (qtd <= 0)
        {
            Vencer();
        }
    }

    private void OnMouseDown()
    {
        StartCoroutine(fazBarra());
        //Debug.Log("2");
    }


    private void OnMouseUp()
    {
        if (fazendoBarra)
        {
            anim.SetBool("Suba",false);
            qtd -= 1;
            barra.text = qtd.ToString();
        }
        else
        {
            naofezBarra = true;
        }
    }
    IEnumerator fazBarra()
    {
        fazendoBarra = false;
        anim.SetBool("Suba", true);
        yield return new WaitForSeconds(0.137f);
        if (naofezBarra == true)
        {
            anim.SetBool("Suba", false);
            qtd -= 1;
            barra.text = qtd.ToString();
        }
        fazendoBarra = true;
        naofezBarra = false;
    }

}
