using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapaBS : MonoBehaviour
{
    public static AudioSource _audio;
    public static List<Vector3Int> notes;
    public static float beatinterval;
    public static int lastbeat = 0;
    public static float beatCount = 0;

    public static bool missable = false;
    public static bool anim = false;
    public static bool verificacao = false;

    // Start is called before the first frame update
    void Start()
    {
        _audio = GetComponent<AudioSource>();
        //StartCoroutine(delayMusica());
        Debug.Log("Contagem comecada!");
        beatinterval = 60f / 154f;
        notes = new List<Vector3Int>();
        Mapeamento();
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {

        if (Mathf.FloorToInt(_audio.time / beatinterval) != lastbeat)
        {
            missable = false;
            anim = true;
            verificacao = true;

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
                missable = true;
            }

            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i][1] == 1 && beatCount % 2 == 1)
                {
                    notes[i] = notes[i] - new Vector3Int(1, 0, 0);
                    //Debug.Log(notes[i]);
                }
                else if (notes[i][1] == 2)
                {
                    notes[i] = notes[i] - new Vector3Int(1, 0, 0);
                    //Debug.Log(notes[i]);
                }
            }
            //Debug.Log(notes[0]);
        }
    }

    private void Mapeamento()
    {

        notes.Add(new Vector3Int(8, 1, 1));
        notes.Add(new Vector3Int(28, 2, 2));
        notes.Add(new Vector3Int(24, 1, 1));
        notes.Add(new Vector3Int(52, 2, 1));
        notes.Add(new Vector3Int(32, 1, 1));
        notes.Add(new Vector3Int(34, 1, 1));
        notes.Add(new Vector3Int(36, 1, 1));
        notes.Add(new Vector3Int(70, 1, 1));
        notes.Add(new Vector3Int(80, 1, 1));
    }
}
