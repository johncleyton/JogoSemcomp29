using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShakeController : MonoBehaviour
{
    [Header("Sensibilidade")]
    [SerializeField] private float shakeThreshold = 2.0f;
    // Quantos segundos de agitação contínua pra encher a barra (menor = mais fácil)
    [SerializeField] private float segundosParaEscapar = 2.0f;
    // Quantos segundos a barra cheia leva pra esvaziar sozinha quando o jogador para (maior = mais fácil)
    [SerializeField] private float segundosParaEsvaziar = 6.0f;
    // Mantém o estado "agitando" por um instante, evita piscar entre sprites a cada frame
    [SerializeField] private float shakeHoldTime = 0.15f;
    [SerializeField] private float shakeAmount = 0.1f;

    public Slider liberationSlider; // opcional: o UvinhaHUD já desenha a barra sozinho

    [Header("Expressões da Uvinha")]
    public SpriteRenderer spriteRenderer;
    public Sprite spriteIdle;      // Arraste o 0 ou 1
    public Sprite spriteRemexer;   // Arraste o 2
    public Sprite spriteAgonia;    // Arraste o 8 ou 9
    public Sprite spriteVitoria;   // Arraste o 6

    private float currentProgress = 0f;
    private float shakeTimer = 0f;
    private Vector3 lowPassValue = Vector3.zero;
    private float lowPassFilter = 0.1f;
    private bool isGameOver = false;
    private Vector3 origemLocal;
    private Vector3 escalaOriginal;

    /// <summary>Progresso de liberdade de 0 a 1 (usado pelo HUD).</summary>
    public float Progress01 => currentProgress / 100f;
    /// <summary>True enquanto o jogador está agitando (usado pelo HUD).</summary>
    public bool Agitando => !isGameOver && shakeTimer > 0f;

    void Start()
    {
        origemLocal = transform.localPosition;
        escalaOriginal = transform.localScale;
        lowPassValue = Input.acceleration;
        if (liberationSlider != null) liberationSlider.value = 0;
        if (spriteRenderer != null) spriteRenderer.sprite = spriteIdle;

        StartCoroutine(PopEntrada());
    }

    // A uvinha "pula" pra dentro da cena em vez de simplesmente estar lá
    private IEnumerator PopEntrada()
    {
        float t = 0f;
        const float dur = 0.4f;
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur) - 1f;
            float s = 1f + c3 * k * k * k + c1 * k * k;
            transform.localScale = escalaOriginal * Mathf.Max(0.01f, s);
            yield return null;
        }
        transform.localScale = escalaOriginal;
    }

    void Update()
    {
        if (isGameOver) return;

        Vector3 accel = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, accel, lowPassFilter);
        Vector3 deltaAccel = accel - lowPassValue;

        bool agitando = deltaAccel.sqrMagnitude >= shakeThreshold * shakeThreshold;

        // SIMULAÇÃO no editor: segurar espaço = agitando
        if (Application.isEditor && Input.GetKey(KeyCode.Space)) agitando = true;

        if (agitando) shakeTimer = shakeHoldTime;
        else shakeTimer -= Time.deltaTime;

        if (shakeTimer > 0f)
        {
            currentProgress += (100f / Mathf.Max(0.1f, segundosParaEscapar)) * Time.deltaTime;
            VisualJuiceEffect();

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = spriteRemexer;
                spriteRenderer.flipX = Random.value > 0.5f;
            }
        }
        else
        {
            currentProgress -= (100f / Mathf.Max(0.1f, segundosParaEsvaziar)) * Time.deltaTime;

            // Medo: quanto mais perto a tesoura, mais a uvinha treme parada
            float medo = UvinhaManager.Instance != null ? Mathf.InverseLerp(0.5f, 1f, UvinhaManager.Instance.TesouraProgresso) : 0f;
            transform.localPosition = origemLocal + (Vector3)(Random.insideUnitCircle * 0.03f * medo);

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = spriteIdle;
                spriteRenderer.flipX = false;
            }
        }

        currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
        if (liberationSlider != null) liberationSlider.value = currentProgress / 100f;

        if (currentProgress >= 100f) WinGame();
    }

    // Treme em volta da posição original
    private void VisualJuiceEffect()
    {
        transform.localPosition = origemLocal + new Vector3(
            Random.Range(-shakeAmount, shakeAmount),
            Random.Range(-shakeAmount, shakeAmount),
            0);
    }

    private void WinGame()
    {
        isGameOver = true;
        transform.localPosition = origemLocal;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = spriteVitoria;
            spriteRenderer.flipX = false;
        }

        if (UvinhaManager.Instance != null) UvinhaManager.Instance.UvaEscapou();
    }

    public void TriggerLose()
    {
        isGameOver = true;
        transform.localPosition = origemLocal;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = spriteAgonia;
            spriteRenderer.flipX = false;
        }
    }
}