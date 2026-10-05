using System.Collections;
using UnityEngine;

public class BalaoVitoria : MonoBehaviour
{
    [Header("Ida pro centro")]
    public float tempoIda = 0.8f;
    public float escalaFinal = 2.5f;

    [Header("Flutuar")]
    public float amplitude = 0.2f;
    public float velocidade = 2f;
    public float balancoRotacao = 5f;

    public void Iniciar()
    {
        StartCoroutine(IrParaCentro());
    }

    private IEnumerator IrParaCentro()
    {
        // Garante que o balão fique na frente de tudo
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 100;

        Camera cam = Camera.main;
        Vector3 origem = transform.position;
        Vector3 destino = new Vector3(cam.transform.position.x, cam.transform.position.y, origem.z);

        Vector3 escalaInicial = transform.localScale;
        Vector3 escalaAlvo = escalaInicial * escalaFinal;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / tempoIda;
            float suave = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(origem, destino, suave);
            transform.localScale = Vector3.Lerp(escalaInicial, escalaAlvo, suave);
            yield return null;
        }

        transform.position = destino;
        transform.localScale = escalaAlvo;

        StartCoroutine(Flutuar(destino));
    }

    private IEnumerator Flutuar(Vector3 centro)
    {
        float tempo = 0f;
        while (true)
        {
            tempo += Time.deltaTime;

            float offsetY = Mathf.Sin(tempo * velocidade) * amplitude;
            transform.position = centro + new Vector3(0f, offsetY, 0f);

            float rot = Mathf.Sin(tempo * velocidade * 0.7f) * balancoRotacao;
            transform.rotation = Quaternion.Euler(0f, 0f, rot);

            yield return null;
        }
    }
}