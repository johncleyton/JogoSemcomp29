using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JogarFlores : MinigameBase
{
    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        jogoAcabou = -1;
        PerderComAtraso(2.0f);
    }

    [SerializeField] GameObject flor;
    private GameObject[] spawnedFlor;
    private int tam = 0;
    private float scale;
    public Vector3 coord;
    private int qtd = Mathf.RoundToInt((200 / Mathf.Pow(1.38f, GameManagerRework.Instance.tempoDoMinigameAtual)));
    public TextMeshProUGUI textQtdFlor;

    public int jogoAcabou = 0;

    // Start is called before the first frame update
    void Start()
    {
        coord = new Vector3(0f, -7f, -5f);
        spawnedFlor = new GameObject[50];
        textQtdFlor.text = qtd.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(jogarFlor());
        }

        if (tam == Mathf.RoundToInt((200 / Mathf.Pow(1.38f, GameManagerRework.Instance.tempoDoMinigameAtual))))
        {
            jogoAcabou = 1;
            VencerComAtraso(2.0f);
        }
    }

    IEnumerator jogarFlor()
    {
        coord.x = Random.Range(-3f, 3f);
        spawnedFlor[tam] = Instantiate(flor, coord, Quaternion.identity);
        scale = Random.Range(0.7f, 1.5f);
        spawnedFlor[tam].transform.localScale = new Vector3(scale, scale, scale);
        spawnedFlor[tam].GetComponent<Rigidbody2D>().drag = Random.Range(0.5f, 1.3f);
        spawnedFlor[tam].GetComponent<Rigidbody2D>().AddForce(new Vector3(Random.Range(-7f,7f),7f,0f), ForceMode2D.Impulse);
        qtd--;
        textQtdFlor.text = qtd.ToString();
        tam++;
        yield return null;
    }


}
