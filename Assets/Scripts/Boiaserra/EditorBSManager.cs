using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class EditorBSManager : MonoBehaviour
{
    public GameObject[] boiaserra;
    public Animator[] animator;
    public AudioSource _audio;

    private int beatCount = 0;
    private float beatinterval;
    private int lastbeat = 0;
    private float bpm = 210f;
    private List<Vector2Int> notasColocadas;
    private int tamanhoNotas = 0;
    private int looping = 0;
    private float lastAudioTime;

    private string path = "./Assets/Scripts/Boiaserra/mapas/mapa1.csv";

    // Start is called before the first frame update
    void Start()
    {
        beatinterval = 60f / bpm;
        File.WriteAllText(path,"");
        notasColocadas = new List<Vector2Int>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_audio.time < lastAudioTime)
        {
            looping++;
            lastAudioTime = _audio.time;
        }

        if (Mathf.FloorToInt((_audio.time+(looping*_audio.clip.length)) / beatinterval) != lastbeat)
        {
            lastAudioTime = _audio.time;
            lastbeat = Mathf.FloorToInt((_audio.time + (looping * _audio.clip.length)) / beatinterval);

            if (beatCount == 4)
            {
                beatCount = 1;
            }
            else
            {
                beatCount += 1;
            }

        }

        if (beatCount % 2 == 0)
        {
            animator[0].SetBool("Levantou", false);
            animator[1].SetBool("Levantou", false);
            animator[2].SetBool("Levantou", false);
            animator[3].SetBool("Levantou", false);

        }
        else if (beatCount % 2 == 1)
        {
            animator[0].SetBool("Levantou", true);
            animator[1].SetBool("Levantou", true);
            animator[2].SetBool("Levantou", true);
            animator[3].SetBool("Levantou", true);
        }

        if (Input.GetMouseButtonDown(0))
        {
            AddNota(1);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            AddNota(2);
        }

    }

    private void AddNota(int tipoNota)
    {
        int beatnota = 0;
        float beatfloat;
        int beatInt;
        if (tipoNota == 1)
        {
            beatfloat = (_audio.time + (looping * _audio.clip.length)) % (beatinterval*2);
            beatInt = (int)((_audio.time + (looping * _audio.clip.length)) /(beatinterval*2));  
            if (beatfloat >= beatinterval)
            {
                beatnota = beatInt + 1;
            }
            else
            {
                beatnota = beatInt;
            }

            if (tamanhoNotas > 0)
            {
                if (notasColocadas[tamanhoNotas - 1][1] == 1)
                {
                    if (beatnota == notasColocadas[tamanhoNotas - 1][0])
                    {
                        Debug.Log("Nota nao colocada: notas iguais");
                        return;
                    }
                }
                else
                {
                    if (notasColocadas[tamanhoNotas - 1][0]%2 != 1 && beatnota == (notasColocadas[tamanhoNotas - 1][0] / 2))
                    {
                        Debug.Log("Nota nao colocada: notas iguais");
                        return;
                    }
                }
            }

            for (int i = tamanhoNotas >= 5 ? tamanhoNotas - 5 : 0; i < tamanhoNotas; i++)
            {
                if (notasColocadas[i][1] == 2)
                {
                    int k = notasColocadas[i][0] % 2 == 1 ? (notasColocadas[i][0] - 1) / 2 : notasColocadas[i][0] / 2;
                    for (int j = 1; j < 3; j++)
                    {
                            if (beatnota - j - Mathf.FloorToInt(j/2) == k - j + 1)
                            {
                            Debug.Log("Nota nao colocada: mesmo boi fazendo duas animacoes ao mesmo tempo");
                            return;
                            }
                    }
                }
            }

        }
        else
        {
            beatfloat = (_audio.time + (looping * _audio.clip.length)) % beatinterval;
            beatInt = (int)((_audio.time + (looping * _audio.clip.length)) /beatinterval);
            if (beatfloat >= beatinterval/2)
            {
                beatnota = beatInt + 1;
            }
            else
            {
                beatnota = beatInt;
            }
               
            if (tamanhoNotas > 0)
            {
                if (notasColocadas[tamanhoNotas - 1][1] == 2)
                {
                    if (beatnota == notasColocadas[tamanhoNotas - 1][0])
                    {
                        Debug.Log("Nota nao colocada: notas iguais");
                        return;
                    }
                }
                else
                {
                    if (beatnota == notasColocadas[tamanhoNotas - 1][0] * 2)
                    {
                        Debug.Log("Nota nao colocada: notas iguais");
                        return;
                    }
                }
            }
            /*
            for (int i = tamanhoNotas >= 5 ? tamanhoNotas - 5 : 0; i < tamanhoNotas; i++)
            {
                if (notasColocadas[i][1] == 1)
                {
                    int k = beatnota % 2 == 1 ? (beatnota - 1) / 2 : beatnota / 2;
                    for (int j = 1; j < 4; j++)
                    {
                            if (k - j + 1 == notasColocadas[i][0] - j)
                            {
                                Debug.Log("nota nao colocada2");
                                return;
                            }
                    }
                }
            }
            */
        }
        


        notasColocadas.Add(new Vector2Int(beatnota, tipoNota));
        tamanhoNotas++;
        Debug.Log(new Vector2Int(beatnota, tipoNota));

        string nota = "notes.Add(new Vector2Int(" + beatnota + ","+ tipoNota +"));";

        File.AppendAllText(path, nota + Environment.NewLine);
    }
}
