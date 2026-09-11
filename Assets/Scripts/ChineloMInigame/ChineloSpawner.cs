using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChineloUISpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject viradoPrefab;
    public GameObject normalPrefab;

    [Header("Área de Spawn")]
    public RectTransform areaDeSpawn;

    [Header("Configurações de Visual")]
    [Range(0.1f, 0.9f)]
    public float proporcaoOcupacao = 0.6f;

    private RectTransform areaDoJogo;
    private List<GameObject> chinelosParaSpawnar;
    private List<Vector2> posicoesDoGrid;

    private List<GameObject> chinelosEmJogo = new List<GameObject>();

    void Awake()
    {
        int tamanho = Random.Range(4, 9);
        int limiteVirados = Mathf.CeilToInt(tamanho / 2f) + 1;
        int qtdVirados = Random.Range(2, limiteVirados);

        chinelosParaSpawnar = new List<GameObject>(tamanho);

        for (int i = 0; i < qtdVirados; i++) chinelosParaSpawnar.Add(viradoPrefab);
        for (int i = qtdVirados; i < tamanho; i++) chinelosParaSpawnar.Add(normalPrefab);

        EmbaralharLista(chinelosParaSpawnar);

        areaDoJogo = gameObject.GetComponent<RectTransform>();

        if (areaDeSpawn == null) areaDeSpawn = areaDoJogo;
    }

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        CalcularGridUI();
        SpawnarChinelosUI();
    }

    private void CalcularGridUI()
    {
        posicoesDoGrid = new List<Vector2>();

        int colunas = 3;
        int linhas = 3;

        float larguraTotal = areaDeSpawn.rect.width;
        float alturaTotal = areaDeSpawn.rect.height;

        float larguraCelula = larguraTotal / colunas;
        float alturaCelula = alturaTotal / linhas;

        Vector2 offsetCentro = GetOffsetRelativoAoPai();

        float startX = -larguraTotal / 2f + offsetCentro.x;
        float startY = -alturaTotal / 2f + offsetCentro.y;

        for (int x = 0; x < colunas; x++)
        {
            for (int y = 0; y < linhas; y++)
            {
                float posX = startX + (x * larguraCelula) + (larguraCelula / 2f);
                float posY = startY + (y * alturaCelula) + (alturaCelula / 2f);

                posicoesDoGrid.Add(new Vector2(posX, posY));
            }
        }

        EmbaralharLista(posicoesDoGrid);
    }

    private Vector2 GetOffsetRelativoAoPai()
    {
        if (areaDeSpawn == areaDoJogo) return Vector2.zero;

        Vector3 centroMundo = areaDeSpawn.TransformPoint(areaDeSpawn.rect.center);
        Vector3 centroLocalNoPai = areaDoJogo.InverseTransformPoint(centroMundo);

        return new Vector2(centroLocalNoPai.x, centroLocalNoPai.y);
    }

    private void SpawnarChinelosUI()
    {
        float larguraCelula = areaDeSpawn.rect.width / 3f;
        float alturaCelula = areaDeSpawn.rect.height / 3f;

        float tamanhoBase = Mathf.Min(larguraCelula, alturaCelula) * proporcaoOcupacao;

        float limiteDesvioX = (larguraCelula - tamanhoBase) / 2f;
        float limiteDesvioY = (alturaCelula - tamanhoBase) / 2f;

        for (int i = 0; i < chinelosParaSpawnar.Count; i++)
        {
            Vector2 centroDaCelula = posicoesDoGrid[i];

            float desvioX = Random.Range(-limiteDesvioX, limiteDesvioX);
            float desvioY = Random.Range(-limiteDesvioY, limiteDesvioY);

            Vector2 posicaoFinal = new Vector2(centroDaCelula.x + desvioX, centroDaCelula.y + desvioY);
            GameObject chineloInstanciado = Instantiate(chinelosParaSpawnar[i], areaDoJogo, false);

            chinelosEmJogo.Add(chineloInstanciado);

            RectTransform rectChinelo = chineloInstanciado.GetComponent<RectTransform>();

            rectChinelo.anchorMin = new Vector2(0.5f, 0.5f);
            rectChinelo.anchorMax = new Vector2(0.5f, 0.5f);
            rectChinelo.pivot = new Vector2(0.5f, 0.5f);

            rectChinelo.localScale = Vector3.one;
            rectChinelo.sizeDelta = new Vector2(tamanhoBase, tamanhoBase);

            rectChinelo.anchoredPosition = posicaoFinal;
        }
    }

    private void EmbaralharLista<T>(List<T> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T aux = lista[i];
            lista[i] = lista[j];
            lista[j] = aux;
        }
    }

    public bool VerificarFimDoJogo()
    {
        bool resultado = true;

        foreach (GameObject chinelo in chinelosEmJogo)
        {
            if (chinelo != null)
            {
                if (chinelo.GetComponent<ChineloInteracao>().estaVirado)
                {
                    resultado = false;
                }
            }
        }

        return resultado;
    }
}