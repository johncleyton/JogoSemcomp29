using UnityEngine;
using System.Collections.Generic;

public class BochinhaScoreManager : MonoBehaviour
{
    public static BochinhaScoreManager Instance;

    public struct BochaData
    {
        public GameObject bochaObject;
        public string team;
        public float distanceToBolim;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void EvaluateRound(List<GameObject> activeBochas, Transform bolimTransform)
    {
        List<BochaData> listData = new List<BochaData>();
        Vector2 bolimPos = bolimTransform.position;

        foreach (var obj in activeBochas)
        {
            if (obj == null) continue; // segurança caso alguma bola tenha sido destruída

            Vector2 objPos = obj.transform.position;
            float dist = Vector2.Distance(objPos, bolimPos);

            string assignedTeam = obj.name.Contains("TimeA") ? "Time A" : "Time B";
            listData.Add(new BochaData { bochaObject = obj, team = assignedTeam, distanceToBolim = dist });
        }

        listData.Sort((x, y) => x.distanceToBolim.CompareTo(y.distanceToBolim));

        if (listData.Count > 0)
        {
            string equipeVencedora = listData[0].team;
            float menorDistancia = listData[0].distanceToBolim;

            // Destaca a bola (no 2D, trocamos a cor do SpriteRenderer)
            SpriteRenderer sr = listData[0].bochaObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.yellow;

            Debug.Log($"Vencedor da Rodada: {equipeVencedora}! Distância: {menorDistancia:F2}m");

            if (BochinhaGameManager.Instance != null)
            {
                string mensagemResultado = equipeVencedora == "Time A"
                    ? "Você venceu essa rodada!"
                    : "O adversário venceu essa rodada...";

                BochinhaGameManager.Instance.scoreText.text = mensagemResultado;
                // Chama a finalização no Manager principal
                BochinhaGameManager.Instance.FinalizarPartida(equipeVencedora);
            }
        }
    }
}