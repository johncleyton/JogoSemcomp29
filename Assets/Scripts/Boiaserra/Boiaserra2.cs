using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boiaserra2 : MonoBehaviour
{
    public GameObject[] boiaserra;
    public Animator[] animator;

    // Start is called before the first frame update
    void Start()
    {
        while (MapaBS.notes[0][0] <= 3)
        {
            for (int i = 0; i < (MapaBS.notes.Count - 1); i++)
            {
                MapaBS.notes[i] = MapaBS.notes[i + 1];
            }
            //remove a ultima nota para diminuir o count e o for de cima continuar dando certo
            MapaBS.notes.RemoveAt(MapaBS.notes.Count - 1);
        }
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {

        if (MapaBS.anim == true)
        {
            //O que faz o boi empinar (animacao em que vc tem que clicar)
            if (MapaBS.notes[0][0] <= 3)
            {
                if (MapaBS.notes[0][1] == 1)
                {
                    if (MapaBS.beatCount % 2 == 1)
                    {
                        if (animator[MapaBS.notes[0][0]].GetBool("Empinou") == false)
                        {
                            animator[MapaBS.notes[0][0]].SetBool("Empinou", true);
                        }
                        else
                        {
                            animator[MapaBS.notes[0][0]].SetBool("Empinou", false);
                        }
                    }
                }
                else
                {
                    if (animator[MapaBS.notes[0][0]].GetBool("Empinou") == false)
                    {
                        animator[MapaBS.notes[0][0]].SetBool("Empinou", true);
                    }
                    else
                    {
                        animator[MapaBS.notes[0][0]].SetBool("Empinou", false);
                    }
                }
            }
            if (MapaBS.notes[1][0] <= 3)
            {
                if (MapaBS.notes[1][1] == 1)
                {
                    if (MapaBS.beatCount % 2 == 1)
                    {
                        if (animator[MapaBS.notes[1][0]].GetBool("Empinou") == false)
                        {
                            animator[MapaBS.notes[1][0]].SetBool("Empinou", true);
                        }
                        else
                        {
                            animator[MapaBS.notes[1][0]].SetBool("Empinou", false);
                        }
                    }
                }
                else
                {
                    if (animator[MapaBS.notes[1][0]].GetBool("Empinou") == false)
                    {
                        animator[MapaBS.notes[1][0]].SetBool("Empinou", true);
                    }
                    else
                    {
                        animator[MapaBS.notes[1][0]].SetBool("Empinou", false);
                    }
                }
            }

            //O que faz a animacao do boi agachando e levantando
            if (MapaBS.beatCount % 2 == 0)
            {
                animator[0].SetBool("Levantou", false);
                animator[1].SetBool("Levantou", false);
                animator[2].SetBool("Levantou", false);
                animator[3].SetBool("Levantou", false);

            }
            else if (MapaBS.beatCount % 2 == 1)
            {
                animator[0].SetBool("Levantou", true);
                animator[1].SetBool("Levantou", true);
                animator[2].SetBool("Levantou", true);
                animator[3].SetBool("Levantou", true);
            }
            MapaBS.anim = false;
        }
    }
}
