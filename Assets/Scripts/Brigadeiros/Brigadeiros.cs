using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Brigadeiros : MinigameBase
{
    public int qtdBrigadeiro;
    [SerializeField] GameObject brigadeiro;
    [SerializeField] GameObject spawn1;
    [SerializeField] GameObject spawn2;
    [SerializeField] GameObject spawn3;
    [SerializeField] GameObject spawn4;
    [SerializeField] GameObject spawn5;
    [SerializeField] GameObject spawn6;
    [SerializeField] GameObject spawn7;
    [SerializeField] GameObject spawn8;
    [SerializeField] GameObject spawn9;
    [SerializeField] GameObject spawn10;
    [SerializeField] GameObject spawn11;
    [SerializeField] GameObject spawn12;

    List<Vector3> spawnpoint = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        qtdBrigadeiro = UnityEngine.Random.Range(Mathf.RoundToInt(10 - GameManagerRework.Instance.tempoDoMinigameAtual), Mathf.RoundToInt(12 - GameManagerRework.Instance.tempoDoMinigameAtual));

        spawnpoint.Add(spawn1.transform.position);
        spawnpoint.Add(spawn2.transform.position);
        spawnpoint.Add(spawn3.transform.position);
        spawnpoint.Add(spawn4.transform.position);
        spawnpoint.Add(spawn5.transform.position);
        spawnpoint.Add(spawn6.transform.position);
        spawnpoint.Add(spawn7.transform.position);
        spawnpoint.Add(spawn8.transform.position);
        spawnpoint.Add(spawn9.transform.position);
        spawnpoint.Add(spawn10.transform.position);
        spawnpoint.Add(spawn11.transform.position);
        spawnpoint.Add(spawn12.transform.position);

        for (int i = 0; i < qtdBrigadeiro; i++)
        {
            Instantiate(brigadeiro, spawnpoint[i], Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (qtdBrigadeiro == 0)
        {
            Debug.Log("GANHOU");
            Vencer();
        }
    }
}
