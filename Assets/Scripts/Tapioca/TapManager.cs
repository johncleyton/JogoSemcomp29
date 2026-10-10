using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapManager : MinigameBase
{
    public Animator anim;
    private Tapioca tapioca;
    private bool animTocando = false;

    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        GetComponent<Tapioca>().enabled = false;
        anim.SetTrigger("Vitoria");
        Vencer();
    }

    // Start is called before the first frame update
    void Start()
    {
        tapioca = GetComponent<Tapioca>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tapioca.jogoAcabou == -1 && animTocando == false)
        {
            animTocando = true;
            GetComponent<Tapioca>().enabled = false;
            anim.SetTrigger("Derrota");
            Perder();
        }
    }
}
