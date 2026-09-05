using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PescaManager : MinigameBase
{
    public Transform pivoCima;
    public Transform pivoBaixo;
    public Transform peixe;

    float peixePosicao, peixeDestino, peixeVelocidade, peixeTimer;
    public float multiplicador = 3f, suavizar = 1f;

    public Transform gancho;
    float ganchoPosicao, ganchoProgresso, ganchoVelocidade;
    public float ganchoTamanho = 0.1f, ganchoForca = 5f, ganchoPuxar = 0.01f;
    public float ganchoGravidade = 0.005f, ganchoProgressoGradual = 0.1f;

    public SpriteRenderer ganchoSprite;
    public Transform barraProgresso;

    public Animator anim;

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        return 20f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoverPeixe();
        MoverGancho();
        ChecarProgresso();
    }

    public float ganchoFriccao = 0.95f;

    public void MoverGancho()
    {
        if (Input.GetMouseButton(0))
            ganchoVelocidade += ganchoForca * Time.deltaTime;
        ganchoVelocidade -= ganchoGravidade * Time.deltaTime;
        ganchoPosicao += ganchoVelocidade;

        float metadeGancho = ganchoTamanho / 2f;
        float limiteInferior = metadeGancho;
        float limiteSuperior = 1f - metadeGancho;

        if (ganchoPosicao >= limiteSuperior)
        {
            ganchoPosicao = limiteSuperior;
            ganchoVelocidade = 0f;
        }
        else 
        if (ganchoPosicao <= limiteInferior)
        {
            ganchoPosicao = limiteInferior;
            ganchoVelocidade = 0f;
        }

        ganchoVelocidade *= ganchoFriccao;
        gancho.position = Vector3.Lerp(pivoBaixo.position, pivoCima.position, ganchoPosicao);
    }

    public void MoverPeixe()
    {
        peixeTimer -= Time.deltaTime;
        if (peixeTimer < 0f)
        {
            peixeTimer = Random.value * multiplicador;
            peixeDestino = Random.value;
        }

        peixePosicao = Mathf.SmoothDamp(peixePosicao, peixeDestino, ref peixeVelocidade, suavizar);
        peixe.position = Vector3.Lerp(pivoBaixo.position, pivoCima.position, peixePosicao);
    }

    public void ChecarProgresso()
    {
        Vector3 ls = barraProgresso.localScale;
        ls.y = ganchoProgresso;
        barraProgresso.localScale = ls;

        float min = ganchoPosicao - ganchoTamanho / 2;
        float max = ganchoPosicao + ganchoTamanho / 2;

        if (min < peixePosicao && peixePosicao < max)
            ganchoProgresso += ganchoPuxar * Time.deltaTime;
        else
            ganchoProgresso -= ganchoProgressoGradual * Time.deltaTime;
        
        if (ganchoProgresso >= 1f)
        {
            anim.SetTrigger("vencer");
            Vencer();
        }

        ganchoProgresso = Mathf.Clamp(ganchoProgresso, 0f, 1f);
    }
}
