using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QTECircle : MinigameBase
{
    public GameObject qte;
    public Animator animator;
    public RectTransform outerRing;
    public Button centerButton;
    public RectTransform circle;
    public RectTransform canvas;
    public TMP_Text instructionText;
    

    public float startScale = 2f;
    public float endScale = 0.4f;

    public float minScale = 1.45f;
    public float maxScale = 0.75f;

    public float duration = 1.5f;
    private float timer;
    private bool active;

    private void Start()
    {
        centerButton.onClick.AddListener(OnClick);
        qte.SetActive(false);
        
        StartCoroutine(QTELoop());
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        duration = Mathf.Max(0.6f, 1.5f - (faseAtual * 0.05f));
        return tempoGlobalSugerido;
    }

    public void StartQTE()
    {
        timer = 0f;
        active = true;

        qte.SetActive(true);
        RandomizePosition();

        outerRing.localScale = Vector3.one * startScale;
    }

    private IEnumerator QTELoop()
    {
        float waitTime = Random.Range(1.5f, 3f);
        yield return new WaitForSeconds(waitTime);

        instructionText.text = "CUMPRIMENTEM!!!";

        yield return new WaitForSeconds(0.6f);

        StartQTE();   
    }

    private void Update()
    {
        if (!active)
            return;

        timer += Time.deltaTime;
        float t = timer / duration;

        float scale = Mathf.Lerp(startScale, endScale, t);

        outerRing.localScale = Vector3.one * scale;

        if (t >= 1f)
        {
            active = false;
            qte.SetActive(false);
            
            animator.SetTrigger("erro");
            PerderComAtraso(2f);
        }
    }

    private void RandomizePosition()
    {
        Vector2 containerSize = canvas.rect.size;
        
        Vector2 qteSize = circle.rect.size;

        float minX = (-containerSize.x / 2f) + (qteSize.x / 2f);
        float maxX = (containerSize.x / 2f) - (qteSize.x / 2f);
        float minY = (-containerSize.y / 2f) + (qteSize.y / 2f);
        float maxY = (containerSize.y / 2f) - (qteSize.y / 2f);

        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        circle.anchoredPosition = new Vector2(randomX, randomY);
    }

    private void OnClick()
    {
        if (!active)
            return;

        active = false;
        float currentScale = outerRing.localScale.x;

        if(currentScale <= minScale && currentScale >= maxScale)
        {
            animator.SetTrigger("acerto");
            VencerComAtraso(2f);
        }
        else
        {
            animator.SetTrigger("erro");
            PerderComAtraso(2f);
        }

        qte.SetActive(false);
    }
}