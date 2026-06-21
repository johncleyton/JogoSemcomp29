using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BochaScoreManager : MonoBehaviour
{

    private Transform bolimTrans;

    public struct BochaData
    {
        public GameObject bochaObject;
        public string team;
        public float distanceBolim;
    }

    public void EvaluateRound()
    {
        // lembrar de colocar tag
        GameObject[] activeBochas = GameObject.FindGameObjectsWithTag("Bocha");

        // data para repr bocha
        List<BochaData> bochas = new List<BochaData>();

        foreach (var obj in activeBochas)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if(rb != null && rb.maxLinearVelocity > 0.05f)
            {
                Debug.Log("Aguardando todas bochas pararem.");
                return;
            }

            float dist = Vector3.Distance(obj.transform.position, bolimTrans.position);

            // aplicar script de identificação da equipe!
            string assignedTeam = "Time_A";
            bochas.Add(new BochaData{ bochaObject= obj, team = assignedTeam, distanceBolim = dist});
        }

        // ordena por distancia, menor primeiro
        bochas.Sort((x,y) => x.distanceBolim.CompareTo(y.distanceBolim));

        if(bochas.Count > 0)
        {
            Debug.Log($"A Bocha mais próxima pertence ao: {bochas[0].team} a uma distância de {bochas[0].distanceBolim}m");
        
            // implementarpontos sequenciais bocha!!!
        }
    }
}