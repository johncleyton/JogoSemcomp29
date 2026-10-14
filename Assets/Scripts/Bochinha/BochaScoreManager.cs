using UnityEngine;
using System.Collections;
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
        Instance = this;
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
            bool jogadorVenceu = equipeVencedora == "Time A";

            // Destaque: linha do bolim até a bola vencedora + anel piscando em volta dela
            // (antes pintava de amarelo, que já é a cor da bocha do jogador, então quase não aparecia)
            StartCoroutine(DestacarVencedora(listData[0].bochaObject, bolimTransform, jogadorVenceu));

            Debug.Log($"Vencedor da Rodada: {equipeVencedora}! Distância: {menorDistancia:F2}m");

            if (BochinhaGameManager.Instance != null)
            {
                string mensagemResultado = jogadorVenceu
                    ? "Você venceu essa rodada!"
                    : "O adversário venceu essa rodada...";

                BochinhaGameManager.Instance.scoreText.text = mensagemResultado;
                // Chama a finalização no Manager principal
                BochinhaGameManager.Instance.FinalizarPartida(equipeVencedora);
            }
        }
    }

    // ----------------------------------------------------------------------------------
    // Destaque visual
    // ----------------------------------------------------------------------------------

    private IEnumerator DestacarVencedora(GameObject bocha, Transform bolim, bool jogadorVenceu)
    {
        if (bocha == null || bolim == null) yield break;

        Color cor = jogadorVenceu ? new Color(0.35f, 1f, 0.45f) : new Color(1f, 0.35f, 0.35f);

        // Raio do anel = raio do desenho da bola + folga
        float raio = 0.5f;
        SpriteRenderer sr = bocha.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            raio = sr.sprite.bounds.extents.x * sr.transform.lossyScale.x;
        raio += 0.12f;

        LineRenderer linha = CriarLinha("LinhaResultado", 2, 0.06f, false);
        LineRenderer anel = CriarLinha("AnelVencedora", 40, 0.1f, true);

        Vector3 posBocha = bocha.transform.position;
        Vector3 posBolim = bolim.position;
        posBocha.z = 0f; posBolim.z = 0f;

        linha.SetPosition(0, posBolim);
        linha.SetPosition(1, posBocha);
        for (int i = 0; i < anel.positionCount; i++)
        {
            float ang = (i / (float)anel.positionCount) * Mathf.PI * 2f;
            anel.SetPosition(i, posBocha + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * raio);
        }

        // Pisca enquanto o jogo mostra o resultado
        float t = 0f;
        while (linha != null && anel != null)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0.35f, 1f, Mathf.PingPong(t * 3f, 1f));
            Color c = new Color(cor.r, cor.g, cor.b, a);
            linha.startColor = linha.endColor = c;
            anel.startColor = anel.endColor = c;
            yield return null;
        }
    }

    private LineRenderer CriarLinha(string nome, int pontos, float largura, bool fechada)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(transform, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        Shader sh = Shader.Find("Sprites/Default");
        if (sh == null) sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        lr.material = new Material(sh);

        lr.useWorldSpace = true;
        lr.loop = fechada;
        lr.positionCount = pontos;
        lr.startWidth = lr.endWidth = largura;
        lr.numCapVertices = 4;
        lr.sortingOrder = 45;
        return lr;
    }
}