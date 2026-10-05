using UnityEngine;
using UnityEngine.UI;

public class ShakeController : MonoBehaviour
{
    private float shakeThreshold = 2.0f;
    private float progressionMultiplier = 5f;
    private float decayRate = 1.5f;

    public Slider liberationSlider;

    [Header("Expressões da Uvinha")]
    public SpriteRenderer spriteRenderer;
    public Sprite spriteIdle;      // Arraste o 0 ou 1
    public Sprite spriteRemexer;   // Arraste o 2
    public Sprite spriteAgonia;    // Arraste o 8 ou 9
    public Sprite spriteVitoria;   // Arraste o 6

    private float currentProgress = 0f;
    private Vector3 lowPassValue = Vector3.zero;
    private float lowPassFilter = 0.1f;
    private bool isGameOver = false;

    void Start()
    {
        lowPassValue = Input.acceleration;
        if(liberationSlider != null) liberationSlider.value = 0;
        if(spriteRenderer != null) spriteRenderer.sprite = spriteIdle;
    }

    void Update()
    {   
        if(isGameOver) return;


        Vector3 accel = Input.acceleration;
        // SIMULACAO - testar com o Unity Remote!!!!!
        // Simula o shake no PC apertando a barra de espaço
        if (Application.isEditor && Input.GetKey(KeyCode.Space))
        {
            accel = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
}
        lowPassValue = Vector3.Lerp(lowPassValue, accel, lowPassFilter);
        Vector3 deltaAccel = accel - lowPassValue;

        // Se estiver agitando o celular (lutando)
        if(deltaAccel.sqrMagnitude >= shakeThreshold * shakeThreshold)
        {
            currentProgress += progressionMultiplier * Time.deltaTime;
            VisualJuiceEffect();
            
            // Troca pro sprite de remexer (2) e inverte horizontalmente de forma caótica
            if(spriteRenderer != null) 
            {
                spriteRenderer.sprite = spriteRemexer;
                spriteRenderer.flipX = Random.value > 0.5f; 
            }
        }
        else // Se estiver parado
        {
            currentProgress -= decayRate * Time.deltaTime;
            
            // Volta pro idle (0 ou 1) e desfaz a inversão
            if(spriteRenderer != null) 
            {
                spriteRenderer.sprite = spriteIdle;
                spriteRenderer.flipX = false;
            }
        }

        currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
        if(liberationSlider != null) liberationSlider.value = currentProgress / 100f;

        if (currentProgress >= 100f)
        {
            WinGame();
        }
    }

    private void VisualJuiceEffect()
    {
        transform.localPosition = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
    }

    private void WinGame()
    {
        isGameOver = true;
        
        // Sprite de Vitória (6) quando se liberta
        if(spriteRenderer != null) 
        {
            spriteRenderer.sprite = spriteVitoria;
            spriteRenderer.flipX = false;
        }

        if (UvinhaManager.Instance != null)
        {
            UvinhaManager.Instance.UvaEscapou();
        }
    }

    public void TriggerLose()
    {
        isGameOver = true;
        
        // Sprite de Agonia (8 ou 9) quando a tesoura alcança e corta
        if(spriteRenderer != null) 
        {
            spriteRenderer.sprite = spriteAgonia;
            spriteRenderer.flipX = false;
        }
    }
}