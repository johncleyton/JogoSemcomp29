using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boiaserra2 : MonoBehaviour
{
    public GameObject[] boiaserra;
    public Animator[] animator;

    public static bool umUpdate = true;

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
    void Update()
    {

        if (Input.GetMouseButtonDown(0) && !PauseBase.mouseOver) 
        {
            if (MapaBS.notes[0][0] <= 1)
            {
                if (animator[3].GetBool("Empinou") == false)
                {
                    animator[3].SetBool("Empinou", true);
                }
                else
                {
                    animator[3].SetBool("Empinou", false);
                }
            }
            else
            {
                //empinou
                animator[3].SetBool("Empinou", !animator[3].GetBool("Empinou"));
            }
        }
        else if(Input.GetMouseButtonUp(0) && !PauseBase.mouseOver && MapaBS.notes[0][0] > 1)
        {
            //empinou2
            animator[3].SetBool("Empinou", !animator[3].GetBool("Empinou"));
        }

        if (MapaBS.notes[0][0] <= 1 && umUpdate == true)
        {
            animator[3].SetBool("Empinou", animator[0].GetBool("Empinou"));
            umUpdate = false;
        }

        if (MapaBS.anim == true)
        {
            //O que faz o boi empinar (animacao em que vc tem que clicar)
            for (int j = 0; j < 5; j++)
            {
                if (MapaBS.notes[j][0] <= 3 && MapaBS.notes[j][0] > 0)
                {
                    if (MapaBS.notes[j][1] == 1)
                    {
                        if (MapaBS.beatCount % 2 == 1)
                        {
                            if (animator[MapaBS.notes[j][0] - 1].GetBool("Empinou") == false)
                            {
                                animator[MapaBS.notes[j][0] - 1].SetBool("Empinou", true);
                            }
                            else
                            {
                                animator[MapaBS.notes[j][0] - 1].SetBool("Empinou", false);
                            }
                        }
                    }
                    else
                    {
                        if (animator[MapaBS.notes[j][0] - 1].GetBool("Empinou") == false)
                        {
                            animator[MapaBS.notes[j][0] - 1].SetBool("Empinou", true);
                        }
                        else
                        {
                            animator[MapaBS.notes[j][0] - 1].SetBool("Empinou", false);
                        }
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
