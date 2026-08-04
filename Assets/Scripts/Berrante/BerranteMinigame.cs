using UnityEngine;
using UnityEngine.UI;

public class BerranteMinigame : MonoBehaviour
{
    [Header("Referências")]
    public DetectorAudio detector;
    public Image fillBar;
    public Transform cameraTransform;

    [Header("Configuração de áudio")]
    public float audioSens = 5f;
    public float threshold = 0.15f;

    [Header("Configuração do minigame")]
    public float requiredFill = 1f;
    public float fillSpeed = 0.5f;
    public float drainSpeed = 0.3f;
    public float timeLimit = 4f;

    [Header("Tremor de câmera (shake)")]
    public float shakeIntensity = 0.15f;
    public float shakeSpeed = 25f;

    [Header("Fallback sem microfone")]
    public bool forcarFallback = false;
    public KeyCode fallbackKey = KeyCode.Space;
    public float fallbackFillPerPress = 0.08f;

    private float currentFill = 0f;
    private float timer;
    private bool finished = false;
    private Vector3 cameraOriginalPos;
    private bool usandoFallback = false;

    void Start()
    {
        timer = timeLimit;
        if (fillBar != null)
            fillBar.fillAmount = 0f;

        if (cameraTransform != null)
            cameraOriginalPos = cameraTransform.localPosition;

        usandoFallback = forcarFallback || detector == null || Microphone.devices.Length == 0;

        if (usandoFallback)
            Debug.Log("Nenhum microfone encontrado");
    }

    void Update()
    {
        if (finished) return;

        timer -= Time.deltaTime;

        bool soprando;

        if (usandoFallback)
        {
            if (Input.GetKeyDown(fallbackKey))
            {
                currentFill += fallbackFillPerPress;
                soprando = true;
            }
            else
            {
                soprando = false;
            }
        }
        else
        {
            float loudness = detector.getLoudnessMic() * audioSens;
            soprando = loudness >= threshold;

            if (soprando)
                currentFill += fillSpeed * Time.deltaTime;
            else
                currentFill -= drainSpeed * Time.deltaTime;
        }

        currentFill = Mathf.Clamp01(currentFill);

        if (fillBar != null)
        {
            fillBar.fillAmount = currentFill;
            fillBar.color = Color.Lerp(Color.red, Color.green, currentFill);
        }

        if (cameraTransform != null)
        {
            if (soprando)
            {
                float offsetX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeIntensity;
                float offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeIntensity;
                cameraTransform.localPosition = cameraOriginalPos + new Vector3(offsetX, offsetY, 0f);
            }
            else
            {
                cameraTransform.localPosition = cameraOriginalPos;
            }
        }

        if (currentFill >= requiredFill)
            Win();
        else if (timer <= 0)
            Lose();
    }

    void Win()
    {
        finished = true;
        Debug.Log("Venceu — berrante soprado a tempo!");
        if (cameraTransform != null)
            cameraTransform.localPosition = cameraOriginalPos;
    }

    void Lose()
    {
        finished = true;
        Debug.Log("Perdeu — não encheu a barra a tempo!");
        if (cameraTransform != null)
            cameraTransform.localPosition = cameraOriginalPos;
    }
}