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

    public static int looping = 0;
    private float lastAudioTime;
    private bool naoDu = true;

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
        if (_audio.time < lastAudioTime)
        {
            looping++;
            lastAudioTime = _audio.time;
        }

        if ((_audio.time + (looping * _audio.clip.length)) - lastbeat * beatinterval > 0.13f && notes[0][0] == 0 && naoDu)
        {
            missNaoClicou = true;
            naoDu = false;
        }

        if (Mathf.FloorToInt((_audio.time + (looping * _audio.clip.length)) / beatinterval) != lastbeat)
        {
            naoDu = true;
            lastAudioTime = _audio.time;
            anim = true;

            lastbeat = Mathf.FloorToInt((_audio.time + (looping * _audio.clip.length)) / beatinterval);

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
            }

            for (int i = notes.Count-1; i >-1; i--)
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
            //Debug.Log("===============================");
            //Debug.Log(notes[0]);
        }
    }

    private void Mapeamento()
    {
        if (GameManagerRework.Instance.tempoDoMinigameAtual > 5f)
        {
            //mapa facil
            //7*6 + (3+3)*6 = 78 sec    (273beatintervals)
            Facil();
        }
        else if (GameManagerRework.Instance.tempoDoMinigameAtual > 3f)
        {
            //mapa medio
            //5*6 + (3+3)*6 = 63 sec    (220beatintervals)
            Medio();
        }
        else
        {
            //mapa dificil ultra insane extra GUIRTU's boyuhull
            //3*6 + (3+3)*6 = 54 sec    (189beatintervals)
            Guirtusboyuhull();
        }
    }

    private void Facil()
    {
        notes.Add(new Vector2Int(12, 2));
        notes.Add(new Vector2Int(7, 1));
        notes.Add(new Vector2Int(9, 1));
        notes.Add(new Vector2Int(11, 1));
        notes.Add(new Vector2Int(13, 1));
        notes.Add(new Vector2Int(15, 1));
        notes.Add(new Vector2Int(17, 1));
        notes.Add(new Vector2Int(18, 1));
        notes.Add(new Vector2Int(19, 1));
        notes.Add(new Vector2Int(20, 1));
        notes.Add(new Vector2Int(43, 2));
        notes.Add(new Vector2Int(47, 2));
        notes.Add(new Vector2Int(49, 2));
        notes.Add(new Vector2Int(52, 2));
        notes.Add(new Vector2Int(56, 2));
        notes.Add(new Vector2Int(57, 2));
        notes.Add(new Vector2Int(30, 1));
        notes.Add(new Vector2Int(32, 1));
        notes.Add(new Vector2Int(34, 1));
        notes.Add(new Vector2Int(36, 1));
        notes.Add(new Vector2Int(37, 1));
        notes.Add(new Vector2Int(80, 2));
        notes.Add(new Vector2Int(42, 1));
        notes.Add(new Vector2Int(44, 1));
        notes.Add(new Vector2Int(91, 2));
        notes.Add(new Vector2Int(93, 2));
        notes.Add(new Vector2Int(96, 2));
        notes.Add(new Vector2Int(99, 2));
        notes.Add(new Vector2Int(52, 1));
        notes.Add(new Vector2Int(54, 1));
        notes.Add(new Vector2Int(56, 1));
        notes.Add(new Vector2Int(59, 1));
        notes.Add(new Vector2Int(61, 1));
        notes.Add(new Vector2Int(65, 1));
        notes.Add(new Vector2Int(67, 1));
        notes.Add(new Vector2Int(69, 1));
        notes.Add(new Vector2Int(70, 1));
        notes.Add(new Vector2Int(143, 2));
        notes.Add(new Vector2Int(147, 2));
        notes.Add(new Vector2Int(149, 2));
        notes.Add(new Vector2Int(153, 2));
        notes.Add(new Vector2Int(157, 2));
        notes.Add(new Vector2Int(81, 1));
        notes.Add(new Vector2Int(83, 1));
        notes.Add(new Vector2Int(86, 1));
        notes.Add(new Vector2Int(89, 1));
        notes.Add(new Vector2Int(273, 1));
        notes.Add(new Vector2Int(274, 1));
        notes.Add(new Vector2Int(275, 1));
        notes.Add(new Vector2Int(276, 1));
        notes.Add(new Vector2Int(277, 1));
        notes.Add(new Vector2Int(278, 1));
    }
    private void Medio()
    {

    }
    private void Guirtusboyuhull()
    {

    }
}
