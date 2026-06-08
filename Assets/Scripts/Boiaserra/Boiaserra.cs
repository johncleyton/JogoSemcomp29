using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boiaserra : MonoBehaviour
{
    private GameManager gameManager;
    private float bpm = 150f;
    [SerializeField] GameObject boiaserra1;
    [SerializeField] GameObject boiaserra2;
    [SerializeField] GameObject boiaserra3;
    [SerializeField] GameObject boiaserra_player;

    private float beatinterval;
    private float beatCount = 0;
    private float compassCount = 0;
    private float beatTimer = 0f;
    private bool miss = true;

    List<int> notes = new List<int>();



    // Start is called before the first frame update
    void Start()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>();

        beatinterval = 60 / bpm;

        //O int que representa cada nota eh quantos beatinterval precisa para chegar na nota
        //Como o boi da esquerda eh o player, e o boi da direita eh quem comeca a leva de movimento
        //sempre precisamos que o int seja um multiplo de 4
        notes.Add(12);
        notes.Add(20);
        notes.Add(28);
        notes.Add(36);
        notes.Add(44);
    }

    // Update is called once per frame
    void Update()
    {
        beatTimer += Time.deltaTime;

        if (beatTimer > beatinterval)
        {
            beatTimer = 0f;

            //Quando nao sobrar nenhum beatinterval na nota com index 0, a nota eh 
            //substituida pela de index 1, e a de index 1 pela de index 2, e assim por diante...
            if (notes[0] == 0)
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

            //Diminui 1 beatinterval de cada nota
            for (int i = 0; i < notes.Count; i++)
            {
                notes[i] -= 1;
                Debug.Log(notes[i]);
            }
            print("===============================");

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

            //movimentacao dos bois
            if (beatCount == 1)
            {
                boiaserra_player.gameObject.transform.position -= new Vector3(0f, 0.5f, 0f);
                boiaserra1.gameObject.transform.position += new Vector3(0f, 0.5f, 0f);
            }
            else if (beatCount == 2)
            {
                boiaserra1.gameObject.transform.position -= new Vector3(0f, 0.5f, 0f);
                boiaserra2.gameObject.transform.position += new Vector3(0f, 0.5f, 0f);
            }
            else if (beatCount == 3)
            {
                boiaserra2.gameObject.transform.position -= new Vector3(0f, 0.5f, 0f);
                boiaserra3.gameObject.transform.position += new Vector3(0f, 0.5f, 0f);
            }
            else if (beatCount == 4)
            {
                boiaserra3.gameObject.transform.position -= new Vector3(0f, 0.5f, 0f);
                boiaserra_player.gameObject.transform.position += new Vector3(0f, 0.5f, 0f);
            }
        }

        //Janela em que a nota esta disponivel: Quando faltar 2 beatinterval.
        //Clicar em espaco fora dos +-80ms de margem de erro e dentro dos 2 beatinterval, faz o jogador errar
        //Antes dos 2 beatinterval, ele pode clicar a vontade que nao vai fazer nenhuma diferenca
        if (notes[0] <= 2 && Input.GetKeyDown(KeyCode.Space))
        {
            //verifica para o erro de -80ms ate 0ms
            if ((beatinterval - beatTimer) < 0.1 && notes[0] == 1)
            {
                print("deu certo");
                miss = false;
            }
            //verifica para o erro de 0ms ate 80ms
            if (beatTimer < 0.1 && notes[0] == 0)
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

