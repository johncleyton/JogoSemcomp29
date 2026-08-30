using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Tapioca : MinigameBase
{
    //Tapioca vindo: 0.1 - 0.2s
    //Tapioca saindo: 0.1 - 0.2s
    public Animator[] animator;
    private float cooldown;
    private float tempo = 0f;
    private bool vindo, frito;
    private float erro;

    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        Vencer();
    }


    void Start()
    {
        erro = GameManagerRework.Instance.tempoDoMinigameAtual / 10;
        Debug.Log("Primeira tapioca vindo");
        Tapioca_vindo();
    }

    // Update is called once per frame
    void Update()
    {
        tempo += Time.deltaTime;
        //Debug.Log(tempo);

        if (tempo > cooldown && tempo < cooldown + erro) {
            if (!frito)
            {
                animator[2].SetTrigger("Fritou");
                Debug.Log("CLIQUE!");
                frito = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                //ganhou
                Debug.Log("Clicou a tempo!");
                StartCoroutine(Tapioca_saindo());
            }
        }
        else if (tempo > cooldown - erro && tempo < cooldown)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //perdeu
                Perder();
            }
        }
        else if (tempo > cooldown + erro)
        {
            Perder();
        }
    }

    void Tapioca_vindo()
    {
        animator[0].SetBool("Fritado", false);
        cooldown = UnityEngine.Random.Range(GameManagerRework.Instance.tempoDoMinigameAtual/7, GameManagerRework.Instance.tempoDoMinigameAtual/2);
        tempo = 0f;
        frito = false;
        Debug.Log("cooldown: " + cooldown);
        Debug.Log("Tempo para clicar: " + erro);
    }   

    IEnumerator Tapioca_saindo()
    {
        animator[0].SetBool("Fritado", true);
        animator[1].SetTrigger("Fritado");
        Debug.Log("Esperando");
        yield return new WaitForSeconds(0.3f);
        Debug.Log("Esperado");
        Tapioca_vindo();
    }
}
