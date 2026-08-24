using UnityEngine;

// [Opcional] Marcador visual em um ponto da grade onde falta um fio.
public class PiacavaThreadNode : MonoBehaviour
{
    public Vector2Int Coordenada { get; private set; }

    private const float VELOCIDADE_PULSO = 4f;
    private const float AMPLITUDE_PULSO = 0.12f;
    private float _faseAnimacao;
    private Vector3 _escalaBase;

    public void Inicializar(Vector2Int coordenada)
    {
        Coordenada = coordenada;
        _faseAnimacao = Random.Range(0f, Mathf.PI * 2f);
        _escalaBase = transform.localScale;
    }

    private void Update()
    {
        _faseAnimacao += Time.deltaTime * VELOCIDADE_PULSO;
        float pulso = 1f + Mathf.Sin(_faseAnimacao) * AMPLITUDE_PULSO;
        transform.localScale = _escalaBase * pulso;
    }
}
