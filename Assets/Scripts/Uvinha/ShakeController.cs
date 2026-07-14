using UnityEngine;
using UnityEngine.UI;

public class ShakeController : MonoBehaviour
{

    private float shakeThreshold = 2.0f;

    private float progressionMultiplier = 5f;
    private float decayRate = 1.5f;

    private Slider liberationSlider;

    private float currentProgress = 0f;

    private Vector3 lowPassValue = Vector3.zero;
    private float lowPassFilter = 0.1f;
    private bool isGameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        lowPassValue = Input.acceleration;

        if(liberationSlider != null) liberationSlider.value = 0;
        
    }

    // Update is called once per frame
    void Update()
    {   
        if(isGameOver) return;

        Vector3 accel = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, accel, lowPassFilter);
        Vector3 deltaAccel = accel - lowPassValue;

        if(deltaAccel.sqrMagnitude >= shakeThreshold * shakeThreshold)
        {
            currentProgress += progressionMultiplier * Time.deltaTime;
            VisualJuiceEffect();
        }
        else
        {
            currentProgress -= decayRate * Time.deltaTime;
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
        // Aplica uma rotação/escala caótica simulando desespero
        transform.localPosition = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
    }

    private void WinGame()
    {
        isGameOver = true;
        Debug.Log("Uva Salva! Fuja do uveiro!");
        // Chamar GameManager para disparar animação de vitória e transição de cena
    }

    public void TriggerLose()
    {
        isGameOver = true;
        Debug.Log("Game Over: A uva foi podada.");
    }
}