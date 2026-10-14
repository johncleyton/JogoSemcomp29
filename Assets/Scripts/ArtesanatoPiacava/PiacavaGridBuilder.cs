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
    // Contorno (em coordenadas de mundo) da área interna da bolsa onde a trama é tecida.
    // Só existem fios entre pontos que estão dentro dele. Vazio = grade retangular completa.
    public Vector2[] areaPermitida;

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
                AdicionarSlot(slots, CriarSlot(pontoA, pontoB, OrientacaoFio.Horizontal, prefabFioHorizontal));
            }
        }

        for (int coluna = 0; coluna <= colunas; coluna++)
        {
            for (int linha = 0; linha < linhas; linha++)
            {
                var pontoA = new Vector2Int(coluna, linha);
                var pontoB = new Vector2Int(coluna, linha + 1);
                AdicionarSlot(slots, CriarSlot(pontoA, pontoB, OrientacaoFio.Vertical, prefabFioVertical));
            }
        }

        return slots;
    }

    private static void AdicionarSlot(List<ThreadSlot> slots, ThreadSlot slot)
    {
        if (slot != null)
            slots.Add(slot);
    }

    private ThreadSlot CriarSlot(Vector2Int pontoA, Vector2Int pontoB, OrientacaoFio orientacao, GameObject prefab)
    {
        if (!PontoPermitido(pontoA) || !PontoPermitido(pontoB))
            return null;

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

            // O comprimento inclui a espessura do fio, para os fios vizinhos se encontrarem nos nós sem deixar cantos vazios
            Vector3 escala = fio.transform.localScale;
            if (orientacao == OrientacaoFio.Horizontal && largura > 0f)
                escala.x = (tamanhoCelula + altura * escala.y) / largura;
            else if (orientacao == OrientacaoFio.Vertical && altura > 0f)
                escala.y = (tamanhoCelula + largura * escala.x) / altura;
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

    public bool PontoPermitido(Vector2Int ponto)
    {
        if (areaPermitida == null || areaPermitida.Length < 3)
            return true;

        Vector3 p = ObterPosicaoDoPonto(ponto);
        bool dentro = false;
        for (int i = 0, j = areaPermitida.Length - 1; i < areaPermitida.Length; j = i++)
        {
            Vector2 a = areaPermitida[i];
            Vector2 b = areaPermitida[j];
            if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                dentro = !dentro;
        }
        return dentro;
    }

    private void OnDrawGizmosSelected()
    {
        if (areaPermitida == null || areaPermitida.Length < 2)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < areaPermitida.Length; i++)
            Gizmos.DrawLine(areaPermitida[i], areaPermitida[(i + 1) % areaPermitida.Length]);
    }

    public Vector3 ObterPosicaoDoPonto(Vector2Int ponto)
    {
        Vector3 origemMundo = origem != null ? origem.position : transform.position;
        return origemMundo + new Vector3(ponto.x * tamanhoCelula, -ponto.y * tamanhoCelula, 0f);
    }
}
