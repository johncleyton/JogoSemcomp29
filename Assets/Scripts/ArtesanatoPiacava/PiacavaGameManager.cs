using System.Collections.Generic;
using UnityEngine;

// Controlador principal do minigame "Artesanato com Piaçava".
// Monta a trama da bolsa, sorteia os fios faltando de acordo com a dificuldade
// e decide o que fazer com os arrastes reportados pelo PiacavaDragInput.
// O tempo, a barra de tempo, a vitória/derrota e as vidas ficam com o GameManagerRework.
[RequireComponent(typeof(PiacavaDragInput))]
public class PiacavaGameManager : MinigameBase
{
    [Header("Referências")]
    public PiacavaGridBuilder gridBuilder;
    public GameObject prefabNo;
    public PiacavaHandGuide guiaDeMao;
    public PiacavaDragInput dragInput;

    [Header("Dificuldade (varia com a faseAtual do GameManagerRework)")]
    public int furosMinimos = 10;
    public int furosMaximos = 18;
    // Fase em que a dificuldade chega ao máximo (a partir dela, não aumenta mais)
    public int fasesParaDificuldadeMaxima = 10;
    // Tempo disponível por furo: começa mais folgado e vai apertando com as fases
    public float segundosPorFuroInicial = 1.5f;
    public float segundosPorFuroFinal = 1.0f;

    [Header("Interação")]
    public float raioDeSelecao = 0.4f;

    private List<ThreadSlot> _todosOsSlots;
    private readonly List<ThreadSlot> _furosPendentes = new List<ThreadSlot>();
    private readonly Dictionary<Vector2Int, PiacavaThreadNode> _nosAtivos = new Dictionary<Vector2Int, PiacavaThreadNode>();

    private PiacavaThreadNode _noDeOrigem;

    // Chamado pelo GameManagerRework logo após a cena ser carregada: monta a trama e sorteia
    // os furos conforme a fase. O valor retornado é o tempo da fase, controlado pelo GameManagerRework.
    // Quanto maior a fase: mais furos e menos tempo por furo.
    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        float progresso = Mathf.Clamp01((faseAtual - 1f) / Mathf.Max(fasesParaDificuldadeMaxima - 1, 1));
        int furos = Mathf.RoundToInt(Mathf.Lerp(furosMinimos, furosMaximos, progresso));
        float segundosPorFuro = Mathf.Lerp(segundosPorFuroInicial, segundosPorFuroFinal, progresso);
        float tempo = furos * segundosPorFuro;
        Debug.Log($"Piaçava - fase {faseAtual}: {furos} furos, {tempo:0.0}s");

        if (dragInput == null)
            dragInput = GetComponent<PiacavaDragInput>();
        dragInput.AoTentarIniciar = TentarIniciarArraste;
        dragInput.ArrasteFinalizado += FinalizarArraste;

        _todosOsSlots = gridBuilder.ConstruirGrade();
        SortearFuros(furos);
        AtualizarGuiaDeMao();

        return tempo;
    }

    private void SortearFuros(int quantidade)
    {
        var disponiveis = new List<ThreadSlot>(_todosOsSlots);
        quantidade = Mathf.Min(quantidade, disponiveis.Count);

        for (int i = 0; i < quantidade; i++)
        {
            int indice = Random.Range(0, disponiveis.Count);
            ThreadSlot slot = disponiveis[indice];
            disponiveis.RemoveAt(indice);

            slot.faltando = true;
            slot.preenchido = false;
            if (slot.fioVisual != null)
                slot.fioVisual.SetActive(false);

            _furosPendentes.Add(slot);
            ObterOuCriarNo(slot.pontoA);
            ObterOuCriarNo(slot.pontoB);
        }
    }

    private PiacavaThreadNode ObterOuCriarNo(Vector2Int coordenada)
    {
        if (_nosAtivos.TryGetValue(coordenada, out PiacavaThreadNode existente))
            return existente;

        Vector3 posicao = gridBuilder.ObterPosicaoDoPonto(coordenada);
        GameObject objetoNo = prefabNo != null
            ? Instantiate(prefabNo, posicao, Quaternion.identity, gridBuilder.transform)
            : new GameObject("No");
        objetoNo.transform.position = posicao;

        PiacavaThreadNode no = objetoNo.GetComponent<PiacavaThreadNode>();
        if (no == null)
            no = objetoNo.AddComponent<PiacavaThreadNode>();
        no.Inicializar(coordenada);

        _nosAtivos[coordenada] = no;
        return no;
    }

    private PiacavaThreadNode EncontrarNoProximo(Vector3 posicaoMundo, PiacavaThreadNode ignorar)
    {
        PiacavaThreadNode maisProximo = null;
        float menorDistancia = raioDeSelecao;

        foreach (PiacavaThreadNode no in _nosAtivos.Values)
        {
            if (no == ignorar) continue;

            float distancia = Vector2.Distance(no.transform.position, posicaoMundo);
            if (distancia <= menorDistancia)
            {
                menorDistancia = distancia;
                maisProximo = no;
            }
        }

        return maisProximo;
    }

    // Chamado pelo PiacavaDragInput quando o arraste começa: só aceita se houver um nó por perto,
    // e devolve a posição do nó pra linha nascer encaixada nele.
    private Vector3? TentarIniciarArraste(Vector3 posicaoMundo)
    {
        if (jogoFinalizado) return null;

        PiacavaThreadNode no = EncontrarNoProximo(posicaoMundo, null);
        if (no == null) return null;

        _noDeOrigem = no;
        return no.transform.position;
    }

    // Chamado pelo PiacavaDragInput quando o jogador solta o dedo/mouse.
    private void FinalizarArraste(Vector3 posicaoMundo)
    {
        PiacavaThreadNode noDeDestino = EncontrarNoProximo(posicaoMundo, _noDeOrigem);

        if (noDeDestino != null)
            TentarCompletarFio(_noDeOrigem, noDeDestino);

        _noDeOrigem = null;
    }

    private void TentarCompletarFio(PiacavaThreadNode origem, PiacavaThreadNode destino)
    {
        ThreadSlot slot = _furosPendentes.Find(s => !s.preenchido && s.ConectaPontos(origem.Coordenada, destino.Coordenada));
        if (slot == null) return;

        slot.preenchido = true;
        if (slot.fioVisual != null)
            slot.fioVisual.SetActive(true);

        _furosPendentes.Remove(slot);
        RemoverNoSeLivre(origem.Coordenada);
        RemoverNoSeLivre(destino.Coordenada);

        if (_furosPendentes.Count == 0)
        {
            if (guiaDeMao != null)
                guiaDeMao.Esconder();
            Vencer();
        }
        else
        {
            AtualizarGuiaDeMao();
        }
    }

    private void RemoverNoSeLivre(Vector2Int coordenada)
    {
        bool aindaNecessario = _furosPendentes.Exists(s => !s.preenchido && (s.pontoA == coordenada || s.pontoB == coordenada));
        if (aindaNecessario) return;

        if (_nosAtivos.TryGetValue(coordenada, out PiacavaThreadNode no))
        {
            _nosAtivos.Remove(coordenada);
            if (no != null)
                Destroy(no.gameObject);
        }
    }

    private void AtualizarGuiaDeMao()
    {
        if (guiaDeMao == null || _furosPendentes.Count == 0) return;
        Vector3 posicaoAlvo = gridBuilder.ObterPosicaoDoPonto(_furosPendentes[0].pontoA);
        guiaDeMao.ApontarPara(posicaoAlvo);
    }
}
