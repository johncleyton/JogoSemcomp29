using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Brigadeiros : MinigameBase
{
    public override void TempoEsgotado()
    {

        if (jogoFinalizado)
            return;

        int i = 0;
        while (true)
        {
            //Debug.Log(i);
            if (spawned[i] != null)
            {
                Destroy(spawned[i].gameObject);
            }
            if (i == 11)
            {
                break;
            }
            i++;
        }
        charac.Derrota();
        PerderComAtraso(2.0f);
    }


    private DragDropGrav[] dragDrop;
    private Brigadeiro_mouthcontroller charac;

    [SerializeField] GameObject brigadeiro;
    [SerializeField] GameObject[] spawn;
    public GameObject[] spawned;
    public int qtdBrigadeiro;


    // Start is called before the first frame update
    void Awake()
    {
        charac = Object.FindFirstObjectByType<Brigadeiro_mouthcontroller>();
        dragDrop = new DragDropGrav[12];
        qtdBrigadeiro = UnityEngine.Random.Range(Mathf.RoundToInt(10 - GameManagerRework.Instance.tempoDoMinigameAtual), Mathf.RoundToInt(12 - GameManagerRework.Instance.tempoDoMinigameAtual));

        for (int i = 0; i < qtdBrigadeiro; i++)
        {
            spawned[i] = Instantiate(brigadeiro, spawn[i].transform.position, Quaternion.identity);
            dragDrop[i] = spawned[i].GetComponent<DragDropGrav>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (qtdBrigadeiro == 0)
        {
            Debug.Log("GANHOU");
            charac.Vitoria();
            VencerComAtraso(2.0f);
        }
        for (int i = 0; i< qtdBrigadeiro; i++)
        {
            if (dragDrop[i].colisao_chao == true)
            {
                int j = 0;
                while (true)
                {
                    //Debug.Log(j);
                    if (spawned[j] != null)
                    {
                        Destroy(spawned[j].gameObject);
                    }
                    if (j == 11)
                    {
                        break;
                    }
                    j++;
                }
                charac.Derrota();
                PerderComAtraso(2.0f);
            }
        }
    }
}
