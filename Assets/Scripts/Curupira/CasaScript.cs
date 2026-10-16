using System.Collections;
using UnityEngine;

// Colocar em cada casa, junto com SpriteRenderer + BoxCollider2D.
// O fogo é um objeto FILHO da casa (sprite da chama), começa desligado
// e acende quando o Curupira é revelado nesta casa.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class CasaScript : MonoBehaviour
{
    [Header("Fogo (cabelo do Curupira)")]
    [Tooltip("Objeto filho com o sprite da chama. Começa desligado.")]
    public GameObject fogo;

    [Header("Feedback")]
    public float escalaHover = 1.05f;
    public Color corErro = new Color(1f, 0.4f, 0.4f);

    SpriteRenderer sr;
    Vector3 posOriginal;
    Vector3 escalaOriginal;
    Vector3 escalaFogoOriginal = Vector3.one;
    Coroutine rotinaAtual;
    Coroutine rotinaFogo;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        posOriginal = transform.localPosition;
        escalaOriginal = transform.localScale;

        // Se o campo ficou vazio, procura um filho chamado "Fogo"
        if (fogo == null)
        {
            // aceita "Fogo", "Fogo (1)", "Fogo (2)"... (nomes que o Unity gera ao duplicar)
            foreach (Transform filho in transform)
            {
                if (filho.name.StartsWith("Fogo")) { fogo = filho.gameObject; break; }
            }
            if (fogo == null)
                Debug.LogWarning("Casa '" + name + "' não tem um filho cujo nome começa com 'Fogo'.", this);
        }

        if (fogo != null)
        {
            escalaFogoOriginal = fogo.transform.localScale;
            // garante que a chama desenha por cima da casa
            SpriteRenderer srFogo = fogo.GetComponent<SpriteRenderer>();
            if (srFogo != null) srFogo.sortingOrder = sr.sortingOrder + 1;
            fogo.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        //Mudanca para resolver bug com pause
        //Enquanto o jogo estiver pausado, essa funcao nao funciona
        if (!enabled) return;

        if (CurupiraManager.Instance != null)
            CurupiraManager.Instance.VerificarCasa(this);
    }

    void OnMouseEnter()
    {
        //Mudanca para resolver bug com pause
        //Enquanto o jogo estiver pausado, essa funcao nao funciona
        if (!enabled) return;

        if (rotinaAtual == null) transform.localScale = escalaOriginal * escalaHover;
    }

    void OnMouseExit()
    {
        //Mudanca para resolver bug com pause
        //Enquanto o jogo estiver pausado, essa funcao nao funciona
        if (!enabled) return;

        if (rotinaAtual == null) transform.localScale = escalaOriginal;
    }

    // O Curupira está aqui: acende o fogo
    public void MostrarFogo()
    {
        if (fogo != null)
        {
            fogo.SetActive(true);
            if (rotinaFogo != null) StopCoroutine(rotinaFogo);
            rotinaFogo = StartCoroutine(AcenderFogo());
        }
        Reiniciar(Pulo());
    }

    // O jogador clicou nesta casa errada
    public void Tremer()
    {
        Reiniciar(TremerRoutine());
    }

    void Reiniciar(IEnumerator rotina)
    {
        if (rotinaAtual != null) StopCoroutine(rotinaAtual);
        rotinaAtual = StartCoroutine(Envolver(rotina));
    }

    IEnumerator Envolver(IEnumerator rotina)
    {
        yield return rotina;
        rotinaAtual = null;
    }

    IEnumerator AcenderFogo()
    {
        Transform f = fogo.transform;

        // "pop": a chama cresce de 0 até o tamanho normal
        float t = 0f, duracao = 0.25f;
        while (t < duracao)
        {
            t += Time.unscaledDeltaTime;
            f.localScale = escalaFogoOriginal * Mathf.SmoothStep(0f, 1f, t / duracao);
            yield return null;
        }

        // depois fica tremulando
        while (true)
        {
            float s = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 14f);
            f.localScale = new Vector3(escalaFogoOriginal.x * (2f - s), escalaFogoOriginal.y * s, escalaFogoOriginal.z);
            yield return null;
        }
    }

    IEnumerator TremerRoutine()
    {
        sr.color = corErro;
        float duracao = 0.4f, t = 0f;
        while (t < duracao)
        {
            t += Time.unscaledDeltaTime;
            float forca = (1f - t / duracao) * 0.12f;
            transform.localPosition = posOriginal + new Vector3(Random.Range(-forca, forca), 0f, 0f);
            yield return null;
        }
        transform.localPosition = posOriginal;
    }

    IEnumerator Pulo()
    {
        float duracao = 0.4f, t = 0f;
        while (t < duracao)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Sin(t / duracao * Mathf.PI); // 0 -> 1 -> 0
            transform.localScale = escalaOriginal * (1f + 0.12f * k);
            transform.localPosition = posOriginal + new Vector3(0f, 0.25f * k, 0f);
            yield return null;
        }
        transform.localScale = escalaOriginal;
        transform.localPosition = posOriginal;
    }
}