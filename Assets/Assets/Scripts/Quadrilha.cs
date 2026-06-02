using UnityEngine;
using UnityEngine.UI;

public class QTECircle : MonoBehaviour
{

    public RectTransform outerRing;
    public Button centerButton;
    public float startScale = 2f;
    public float endScale = 0.5f;

    public float duration = 1.5f;
    private float timer;
    private bool active;

    private void Start()
    {
        centerButton.onClick.AddListener(OnClick);
        StartQTE();
    }

    public void StartQTE()
    {
        timer = 0f;
        active = true;

        outerRing.localScale = Vector3.one * startScale;
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
            Debug.Log("MISS");
        }
    }

    private void OnClick()
    {
        if (!active)
            return;

        float currentScale = outerRing.localScale.x;

        if(currentScale <= 0.85f && currentScale >= 0.5f)
        {
            Debug.Log("acertou");
        }
        else
        {
            Debug.Log("não");
        }

        active = false;
    }
}