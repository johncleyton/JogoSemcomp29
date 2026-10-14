using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlorManager : MonoBehaviour
{
    public JogarFlores jogarflores;
    public Animator anim;
    private float tempo = -2;

    public GameObject onda;
    private GameObject onda2;
    public int tipodaOnda2 = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //garante que pelo menos nao vai ser igual a ultima onda
        if (onda2 != null && tipodaOnda2 != onda2.GetComponent<OndaFlor>().tipodaOnda)
        {
            tipodaOnda2 = onda2.GetComponent<OndaFlor>().tipodaOnda;
        }

        //spawna uma primeira onda logo no inicio sem instanciar ela no TelaIntervalo
        if (tempo > -1.9 && tempo < 0)
        {
            onda2 = Instantiate(onda, new Vector3(0f, 7f, -4.55f), Quaternion.identity);
            tempo += 2;
        }
        
        tempo += Time.deltaTime;
        if (tempo > 2)
        {
            onda2 = Instantiate(onda, new Vector3(0f, 7f, -4.55f),Quaternion.identity);
            tempo = 0;
        }


        if (jogarflores.jogoAcabou == 1)
        {
            jogarflores.enabled = false;
            anim.SetTrigger("vitoria");
        }
        else if (jogarflores.jogoAcabou == -1)
        {
            jogarflores.enabled = false;
            anim.SetTrigger("derrota");
        }
    }
}
