using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Verificaderrota : MinigameBase
{
    
    private bool miss = true;

    public Boiaserra2 boiaserra2;
    private int jogoAcabou = 0;
    public Animator anim;

    //public GameObject bg;
    public GameObject voce;

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        float tempoFixo = 10f;

        return tempoFixo;
    }
    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        jogoAcabou = 1;
        VitoriaDerrota();
        VencerComAtraso(2.0f);
    }

    private void VitoriaDerrota()
    {
        boiaserra2.enabled = false;
        //bg.transform.position = new Vector3(-0.6652f, -0.6287f, -9.0f);
        //bg.transform.localScale = new Vector3(3.3f, 3.3f, 3.3f) * 1f;
        voce.SetActive(false);
        if (jogoAcabou == 1)
        {
            anim.SetTrigger("vitoria");
        }
        else if (jogoAcabou == -1)
        {
            anim.SetTrigger("derrota");
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        MapaBS.missNaoClicou = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (jogoAcabou == 0)
        {
            //Janela em que a nota esta disponivel: Quando faltar 2 beatinterval.
            //Clicar em espaco fora dos +-150ms de margem de erro e dentro dos 2 beatinterval, faz o jogador errar
            //Antes dos 2 beatinterval, ele pode clicar a vontade que nao vai fazer nenhuma diferenca
            if (Input.GetMouseButtonDown(0) && !PauseBase.mouseOver)
            {
                //Debug.Log("clicou------------");
                if (MapaBS.notes[0][0] <= 2)
                {
                    if (MapaBS.notes[0][0] <= 1)
                    {
                        //verifica para o erro de -150ms ate 0ms
                        if (MapaBS.beatinterval - ((MapaBS._audio.time + (MapaBS.looping * MapaBS._audio.clip.length)) - MapaBS.lastbeat * MapaBS.beatinterval) < 0.13f)
                        {
                            print("deu certo1");
                            miss = false;
                        }
                        //verifica para o erro de 0ms ate 150ms
                        else if ((MapaBS._audio.time + (MapaBS.looping * MapaBS._audio.clip.length)) - MapaBS.lastbeat * MapaBS.beatinterval < 0.13f && MapaBS.notes[0][0] == 0)
                        {
                            print("deu certo2");
                            miss = false;
                        }
                    }
                    if (miss == true)
                    {
                        print("FALHOUU");
                        jogoAcabou = -1;
                        VitoriaDerrota();
                        PerderComAtraso(2.0f);
                    }
                }
            }

            if (MapaBS.missNaoClicou == true)
            {
                //Caso o jogador nao tenha clicado na janela em que a nota estava disponivel, ele erra
                //e perde
                //Caso o jogador tenha acertado, miss se torna false, e entao aqui ele volta a ser true
                if (miss)
                {
                    Debug.Log("Nao clicou");
                    jogoAcabou = -1;
                    VitoriaDerrota();
                    PerderComAtraso(2.0f);
                }
                else
                {
                    miss = true;
                    MapaBS.missNaoClicou = false;
                }
            }
        }
    }
}
