using UnityEngine;

// Indicador opcional (uma "mão") que flutua sobre o próximo furo a ser costurado.
public class PiacavaHandGuide : MonoBehaviour
{
    public float velocidadeFlutuacao = 2f;
    public float amplitudeFlutuacao = 0.15f;

    private Vector3 _posicaoBase;
    private bool _alvoDefinido;

    public void ApontarPara(Vector3 posicaoMundo)
    {
        _posicaoBase = posicaoMundo;
        _alvoDefinido = true;
        gameObject.SetActive(true);
    }

    public void Esconder()
    {
        _alvoDefinido = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_alvoDefinido) return;
        float deslocamento = Mathf.Sin(Time.time * velocidadeFlutuacao) * amplitudeFlutuacao;
        transform.position = _posicaoBase + new Vector3(0f, deslocamento, 0f);
    }
}
