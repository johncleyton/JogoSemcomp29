using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Verificaderrota : MinigameBase
{
    
    private bool miss = true;

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        float tempoFixo = 60f;

        return tempoFixo;
    }
    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        Vencer();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    
        //Janela em que a nota esta disponivel: Quando faltar 2 beatinterval.
        //Clicar em espaco fora dos +-100ms de margem de erro e dentro dos 2 beatinterval, faz o jogador errar
        //Antes dos 2 beatinterval, ele pode clicar a vontade que nao vai fazer nenhuma diferenca
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("clicou------------");
            if (MapaBS.notes[0][0] <= 2)
            {
                if (MapaBS.notes[0][0] <= 1)
                {
                    //verifica para o erro de -100ms ate 0ms
                    if (MapaBS.beatinterval - (MapaBS._audio.time - MapaBS.lastbeat * MapaBS.beatinterval) < 0.3f)
                    {
                        print("deu certo1");
                        miss = false;
                    }
                    //verifica para o erro de 0ms ate 100ms
                    else if (MapaBS._audio.time - MapaBS.lastbeat * MapaBS.beatinterval < 0.3f)
                    {
                        print("deu certo2");
                        miss = false;
                    }
                }
                if (miss == true)
                {
                    print("FALHOUU");
                    Perder();
                }
            }
        }

        if (MapaBS.verificacao == true)
        {
            if (MapaBS.missable == true)
            {
                //Caso o jogador nao tenha clicado na janela em que a nota estava disponivel, ele erra
                //e perde
                //Caso o jogador tenha acertado, miss se torna false, e entao aqui ele volta a ser true
                if (miss)
                {
                    Debug.Log("Nao clicou");
                    Perder();
                }
                else
                {
                    miss = true;
                }
            }
            MapaBS.verificacao = false;
        }
    }
}
