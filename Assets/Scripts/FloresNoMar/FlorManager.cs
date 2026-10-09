using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlorManager : MonoBehaviour
{
    public JogarFlores jogarflores;
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

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
