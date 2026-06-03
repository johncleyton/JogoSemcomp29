using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QTECircle : MonoBehaviour
{
    public GameObject qte;
    public Animator animator;
    public RectTransform outerRing;
    public Button centerButton;
    public RectTransform circle;
    public TMP_Text instructionText;
    public float startScale = 2f;
    public float endScale = 0.4f;

    public float minScale = 1.45f;
    public float maxScale = 0.75f;

    public float duration = 1.5f;
    private float timer;
    private bool active;

    private string[] commands =
    {
        "CUMPRIMENTEM!!!!",
        "É MENTIRA!!!!",
        "TROCA DE PAR!!!!"
    };

    private void Start()
    {
        centerButton.onClick.AddListener(OnClick);
        qte.SetActive(false);
        
        StartCoroutine(QTELoop());
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
        string command = commands[Random.Range(0, commands.Length)];
        float waitTime = Random.Range(1.5f, 3f);

        yield return new WaitForSeconds(waitTime);

        instructionText.text = command;

        yield return new WaitForSeconds(0.8f);

        //instructionText.text = "";

        StartQTE();

        yield return new WaitUntil(() => !active);

        yield return new WaitForSeconds(1f);
        
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
            Debug.Log("não");
        }
    }

    private void RandomizePosition()
    {
        float x = Random.Range(-450f, 410f);
        float y = Random.Range(260f, -180f);

        circle.anchoredPosition = new Vector2(x, y);
    }

    private void OnClick()
    {
        if (!active)
            return;

        float currentScale = outerRing.localScale.x;

        if(currentScale <= minScale && currentScale >= maxScale)
        {
            Debug.Log("acertou");
            animator.SetTrigger("acerto");
        }
        else
        {
            Debug.Log("não");
            animator.SetTrigger("erro");
        }

        active = false;
    }
}