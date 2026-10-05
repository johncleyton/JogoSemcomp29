using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapaBS : MonoBehaviour
{
    public static AudioSource _audio;
    public static List<Vector2Int> notes;

    private float bpm = 210f;
    public static float beatinterval;
    public static int lastbeat = 0;
    public static float beatCount = 0;

    public static bool missNaoClicou = false;
    public static bool anim = false;

    // Start is called before the first frame update
    void Start()
    {
        _audio = GetComponent<AudioSource>();
        //StartCoroutine(delayMusica());
        Debug.Log("Contagem comecada!");
        beatinterval = 60f / bpm;
        notes = new List<Vector2Int>();
        Mapeamento();
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if (Mathf.FloorToInt(_audio.time / beatinterval) != lastbeat)
        {
            anim = true;

            lastbeat = Mathf.FloorToInt(_audio.time / beatinterval);

            if (beatCount == 4)
            {
                beatCount = 1;
            }
            else
            {
                beatCount += 1;
            }

            //Quando nao sobrar nenhum beatinterval na nota com index 0, a nota eh 
            //substituida pela de index 1, e a de index 1 pela de index 2, e assim por diante...
            if (notes[0][0] == 0)
            {
                for (int i = 0; i < (notes.Count - 1); i++)
                {
                    notes[i] = notes[i + 1];
                }
                //remove a ultima nota para diminuir o count e o for de cima continuar dando certo
                notes.RemoveAt(notes.Count - 1);
                missNaoClicou = true;
            }

            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i][1] == 1 && beatCount % 2 == 1)
                {
                    notes[i] = notes[i] - new Vector2Int(1, 0);
                    //Debug.Log(notes[i]);
                }
                else if (notes[i][1] == 2)
                {
                    notes[i] = notes[i] - new Vector2Int(1, 0);
                    //Debug.Log(notes[i]);
                }
            }
            //Debug.Log(notes[0]);
        }
    }

    private void Mapeamento()
    {
        if (GameManagerRework.Instance.tempoDoMinigameAtual > 5f)
        {
            //mapa facil
            //7*6 + (3+3)*6 = 78 sec
            Facil();
        }
        else if (GameManagerRework.Instance.tempoDoMinigameAtual > 3f)
        {
            //mapa medio
            //5*6 + (3+3)*6 = 63 sec
            Medio();
        }
        else
        {
            //mapa dificil ultra insane extra GUIRTU's boyuhull
            //3*6 + (3+3)*6 = 54 sec
            Guirtusboyuhull();
        }
    }

    private void Facil()
    {
        notes.Add(new Vector2Int(8, 1));
        notes.Add(new Vector2Int(10, 1));
        notes.Add(new Vector2Int(22, 2));
        notes.Add(new Vector2Int(14, 1));
        notes.Add(new Vector2Int(273, 1));
        notes.Add(new Vector2Int(274, 1));
    }
    private void Medio()
    {

    }
    private void Guirtusboyuhull()
    {

    }
}
