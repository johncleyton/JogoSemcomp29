using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    public bool isCapivara;
    private bool AlreadyClicked = false;
    private SpriteRenderer sr;

    void Start()
    {
        // MELHORIA: Agora procura o SpriteRenderer tanto no próprio objeto quanto dentro dos filhos!
        sr = GetComponentInChildren<SpriteRenderer>();
        
        if (sr == null)
        {
            Debug.LogWarning("Aviso: Nenhum SpriteRenderer encontrado no objeto " + gameObject.name);
        }
    }

    void OnMouseDown()
    {
        if (AlreadyClicked) return;
        AlreadyClicked = true;

        if (isCapivara)
        {
            StartCoroutine(PopOutAnimation());
        }
        else
        {
            if (GameManagerCapivara.Instance != null)
            {
                GameManagerCapivara.Instance.GameOver();
            }
        }
    }

    System.Collections.IEnumerator PopOutAnimation()
    {
        // 1. Proteção do Sprite
        if (sr != null) 
        {
            sr.sortingOrder = 100;
        }

        Vector3 originalPos = transform.position;
        Vector3 targetPos = originalPos; // Posição padrão caso a câmera falhe

        // 2. Proteção da Câmera
        if (Camera.main != null)
        {
            targetPos = Camera.main.transform.position;
        }
        else
        {
            Debug.LogError("Erro: Nenhuma câmera com a tag 'MainCamera' encontrada!");
        }
        
        targetPos.z = 0f;

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 3f; 

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

        // 3. Proteção do GameManager
        if (GameManagerCapivara.Instance != null)
        {
            GameManagerCapivara.Instance.CapivaraEncontrada();
        }
        
        gameObject.SetActive(false);
    }
}