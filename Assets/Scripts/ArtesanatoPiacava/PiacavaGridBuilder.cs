using System.Collections.Generic;
using UnityEngine;

// Monta em tempo de execução a trama da bolsa (fios horizontais e verticais)
// sobre o sprite de fundo, usando apenas os dois prefabs de fio.
public class PiacavaGridBuilder : MonoBehaviour
{
    [Header("Grade da bolsa")]
    public Transform origem;
    public int colunas = 6;
    public int linhas = 8;
    public float tamanhoCelula = 0.5f;

    [Header("Sprites dos fios")]
    public GameObject prefabFioHorizontal;
    public GameObject prefabFioVertical;

    private Transform _containerFios;

    // Constrói a grade inteira com todos os fios presentes.
    // O controlador do minigame é quem decide, depois, quais slots ficam "faltando".
    public List<ThreadSlot> ConstruirGrade()
    {
        var slots = new List<ThreadSlot>();

        if (_containerFios == null)
        {
            _containerFios = new GameObject("Fios").transform;
            _containerFios.SetParent(transform, false);
        }

        for (int linha = 0; linha <= linhas; linha++)
        {
            for (int coluna = 0; coluna < colunas; coluna++)
            {
                var pontoA = new Vector2Int(coluna, linha);
                var pontoB = new Vector2Int(coluna + 1, linha);
                slots.Add(CriarSlot(pontoA, pontoB, OrientacaoFio.Horizontal, prefabFioHorizontal));
            }
        }

        for (int coluna = 0; coluna <= colunas; coluna++)
        {
            for (int linha = 0; linha < linhas; linha++)
            {
                var pontoA = new Vector2Int(coluna, linha);
                var pontoB = new Vector2Int(coluna, linha + 1);
                slots.Add(CriarSlot(pontoA, pontoB, OrientacaoFio.Vertical, prefabFioVertical));
            }
        }

        return slots;
    }

    private ThreadSlot CriarSlot(Vector2Int pontoA, Vector2Int pontoB, OrientacaoFio orientacao, GameObject prefab)
    {
        Vector3 posicaoMedia = (ObterPosicaoDoPonto(pontoA) + ObterPosicaoDoPonto(pontoB)) * 0.5f;

        GameObject fio;
        if (prefab != null)
        {
            fio = Instantiate(prefab, posicaoMedia, Quaternion.identity, _containerFios);
        }
        else
        {
            fio = new GameObject("Fio");
            fio.transform.SetParent(_containerFios, false);
            fio.transform.position = posicaoMedia;
        }

        // Estica o sprite do fio para preencher exatamente uma célula da grade,
        // independente do tamanho original do sprite importado.
        var renderer = fio.GetComponent<SpriteRenderer>();
        if (renderer != null && renderer.sprite != null)
        {
            float largura = renderer.sprite.bounds.size.x;
            float altura = renderer.sprite.bounds.size.y;

            Vector3 escala = fio.transform.localScale;
            if (orientacao == OrientacaoFio.Horizontal && largura > 0f)
                escala.x = tamanhoCelula / largura;
            else if (orientacao == OrientacaoFio.Vertical && altura > 0f)
                escala.y = tamanhoCelula / altura;
            fio.transform.localScale = escala;
        }

        return new ThreadSlot
        {
            pontoA = pontoA,
            pontoB = pontoB,
            orientacao = orientacao,
            fioVisual = fio,
            faltando = false,
            preenchido = true
        };
    }

    public Vector3 ObterPosicaoDoPonto(Vector2Int ponto)
    {
        Vector3 origemMundo = origem != null ? origem.position : transform.position;
        return origemMundo + new Vector3(ponto.x * tamanhoCelula, -ponto.y * tamanhoCelula, 0f);
    }
}
