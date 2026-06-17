using UnityEngine;

public class ClickableObject : MonoBehaviour
{

    public bool isCapivara;
    private bool AlreadyClicked = false;

    private SpriteRenderer sr;


    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // CORRIGIDO: evita NullReferenceException quando o sprite está
        // num objeto filho (ex: estrutura com sombra ou partes separadas)
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }
    }

    void OnMouseDown()
    {
        // CORRIGIDO: não reage mais a toques depois que o jogo já terminou
        // (vitória ou derrota), evitando pontuar ou perder de novo fora de hora.
        if (GameManagerCapivara.Instance != null && GameManagerCapivara.Instance.JogoEncerrado) return;

        if (AlreadyClicked) return;

        AlreadyClicked = true;

        if (isCapivara)
        {
            StartCoroutine(PopOutAnimation());
        }
        else
        {
            GameManagerCapivara.Instance.GameOver();
        }
    }

    System.Collections.IEnumerator PopOutAnimation()
    {
        if (sr != null) sr.sortingOrder = 100;

        Vector3 originalPos = transform.position;

        // CORRIGIDO: Mudado de Vector para Vector3
        Vector3 targetPos = Camera.main.transform.position;

        // mantem em xy
        targetPos.z = 0f;

        Vector3 originalScale = transform.localScale;

        Vector3 targetScale = originalScale * 3f;
        // fica 3 vezes maior na cara do player

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(originalPos, targetPos, elapsed / duration);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.localScale = targetScale;

        yield return new WaitForSeconds(1.5f);

        GameManagerCapivara.Instance.CapivaraEncontrada();
        gameObject.SetActive(false);
    }
}
