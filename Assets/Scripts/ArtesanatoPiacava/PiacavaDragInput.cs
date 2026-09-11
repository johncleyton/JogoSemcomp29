using System;
using UnityEngine;

// Entrada de arrastar-e-soltar (mouse no desktop, toque no mobile), desacoplada da lógica do jogo.
// Quem usa esse componente decide, via AoTentarIniciar, se um arraste pode começar numa posição e
// de onde a linha deve nascer (ex.: encaixada no nó mais próximo); ArrasteFinalizado avisa onde o
// jogador soltou o dedo/mouse.
[RequireComponent(typeof(LineRenderer))]
public class PiacavaDragInput : MonoBehaviour
{
    public LineRenderer linhaDeArraste;
    public float distanciaDaCamera = 10f;

    public Func<Vector3, Vector3?> AoTentarIniciar;
    public event Action<Vector3> ArrasteAtualizado;
    public event Action<Vector3> ArrasteFinalizado;

    private bool _arrastando;

    private void Awake()
    {
        if (linhaDeArraste == null)
            linhaDeArraste = GetComponent<LineRenderer>();
        linhaDeArraste.positionCount = 0;
        linhaDeArraste.enabled = false;
    }

    // Touch tem prioridade quando presente, senão cai pro mouse.
    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            ProcessarInput(touch.position, touch.phase);
            return;
        }

        if (Input.GetMouseButtonDown(0))
            ProcessarInput(Input.mousePosition, TouchPhase.Began);
        else if (Input.GetMouseButton(0) && _arrastando)
            ProcessarInput(Input.mousePosition, TouchPhase.Moved);
        else if (Input.GetMouseButtonUp(0) && _arrastando)
            ProcessarInput(Input.mousePosition, TouchPhase.Ended);
    }

    private void ProcessarInput(Vector2 posicaoTela, TouchPhase fase)
    {
        Vector3 posicaoMundo = ObterPosicaoNoMundo(posicaoTela);

        switch (fase)
        {
            case TouchPhase.Began:
                IniciarArraste(posicaoMundo);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (_arrastando)
                    AtualizarArraste(posicaoMundo);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (_arrastando)
                    FinalizarArraste(posicaoMundo);
                break;
        }
    }

    private Vector3 ObterPosicaoNoMundo(Vector2 posicaoTela)
    {
        Vector3 posicao = posicaoTela;
        posicao.z = distanciaDaCamera;
        return Camera.main.ScreenToWorldPoint(posicao);
    }

    private void IniciarArraste(Vector3 posicaoMundo)
    {
        Vector3? origem = AoTentarIniciar?.Invoke(posicaoMundo);
        if (origem == null) return;

        _arrastando = true;

        linhaDeArraste.enabled = true;
        linhaDeArraste.positionCount = 2;
        linhaDeArraste.SetPosition(0, origem.Value);
        linhaDeArraste.SetPosition(1, posicaoMundo);
    }

    private void AtualizarArraste(Vector3 posicaoMundo)
    {
        linhaDeArraste.SetPosition(1, posicaoMundo);
        ArrasteAtualizado?.Invoke(posicaoMundo);
    }

    private void FinalizarArraste(Vector3 posicaoMundo)
    {
        _arrastando = false;
        linhaDeArraste.enabled = false;
        linhaDeArraste.positionCount = 0;

        ArrasteFinalizado?.Invoke(posicaoMundo);
    }
}
