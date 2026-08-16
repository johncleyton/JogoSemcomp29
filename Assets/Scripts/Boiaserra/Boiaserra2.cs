using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boiaserra2 : MonoBehaviour
{
    private GameManager gameManager;
    //O bpm real eh dividido por 2, ou seja, 75 bpm
    private float bpm = 300f;
    public GameObject[] boiaserra;
    public Animator[] animator;
    private int boiCount = 0;

    private float beatinterval;
    private float beatCount = 0;
    private float compassCount = 0;
    private float beatTimer = 0f;
    private bool miss = true;

    List<Vector3Int> notes = new List<Vector3Int>();



    // Start is called before the first frame update
    void Start()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>();

        beatinterval = 60 / bpm;

        //O int que representa cada nota eh quantos beatinterval precisa para chegar na nota
        //Como o boi da esquerda eh o player, e o boi da direita eh quem comeca a leva de movimento
        //sempre precisamos que o int seja um multiplo de 4
        notes.Add(new Vector3Int(4, 1, 2));
        notes.Add(new Vector3Int(8, 1, 1));
        notes.Add(new Vector3Int(28, 2, 2));
        notes.Add(new Vector3Int(36, 1, 1));
        notes.Add(new Vector3Int(44, 1, 1));
    }

    // Update is called once per frame
    void Update()
    {
        beatTimer += Time.deltaTime;

        if (beatTimer > beatinterval)
        {
            beatTimer = 0f;

            //fica circulando entre 1 a 4
            if (beatCount == 4)
            {
                beatCount = 1;
                compassCount += 1;
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

                //Caso o jogador nao tenha clicado na janela em que a nota estava disponivel, ele erra
                //e perde
                //Caso o jogador tenha acertado, miss se torna false, e entao aqui ele volta a ser true
                if (miss)
                {
                    print("FALHOU");
                    SceneManager.LoadScene(5);
                }
                else
                {
                    miss = true;
                }
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
            //print("===============================");

            if (notes[0][0] <= 3)
            {
                if (notes[0][1] == 1) 
                {
                    if (beatCount % 2 == 1)
                    {
                        if (animator[notes[0][0]].GetBool("Empinou") == false)
                        {
                            animator[notes[0][0]].SetBool("Empinou", true);
                        }
                        else
                        {
                            animator[notes[0][0]].SetBool("Empinou", false);
                        }
                    }
                }
                else
                {
                    if (animator[notes[0][0]].GetBool("Empinou") == false)
                    {
                        animator[notes[0][0]].SetBool("Empinou", true);
                    }
                    else
                    {
                        animator[notes[0][0]].SetBool("Empinou", false);
                    }
                }
            }
            if (notes[1][0] <= 3)
            {
                if (notes[1][1] == 1)
                {
                    if (beatCount % 2 == 1)
                    {
                        if (animator[notes[1][0]].GetBool("Empinou") == false)
                        {
                            animator[notes[1][0]].SetBool("Empinou", true);
                        }
                        else
                        {
                            animator[notes[1][0]].SetBool("Empinou", false);
                        }
                    }
                }
                else
                {
                    if (animator[notes[1][0]].GetBool("Empinou") == false)
                    {
                        animator[notes[1][0]].SetBool("Empinou", true);
                    }
                    else
                    {
                        animator[notes[1][0]].SetBool("Empinou", false);
                    }
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
        }

        //Janela em que a nota esta disponivel: Quando faltar 2 beatinterval.
        //Clicar em espaco fora dos +-100ms de margem de erro e dentro dos 2 beatinterval, faz o jogador errar
        //Antes dos 2 beatinterval, ele pode clicar a vontade que nao vai fazer nenhuma diferenca
        if (notes[0][0] <= 2 && Input.GetKeyDown(KeyCode.Space))
        {
            //verifica para o erro de -100ms ate 0ms
            if ((beatinterval - beatTimer) < 0.1 && notes[0][0] == 1)
            {
                print("deu certo");
                miss = false;
            }
            //verifica para o erro de 0ms ate 100ms
            if (beatTimer < 0.1 && notes[0][0] == 0)
            {
                print("deu certo");
                miss = false;
            }
            else if (miss == true)
            {
                print("FALHOUU");
                SceneManager.LoadScene(5);
            }
        }
        if (gameManager.timer <= 0)
        {
            Debug.Log("GANHOU");
        }
    }
}
