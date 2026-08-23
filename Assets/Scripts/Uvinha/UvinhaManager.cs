using UnityEngine;

public class UvinhaManager : MinigameBase
{
    public static UvinhaManager Instance;

    [Header("Referências")]
    public Transform tesouraTransform;
    public Transform startPoint, cutPoint;
    public ShakeController uvaController;

    private float tempoDaRodada;
    private float elapsedTime = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // Guardamos o tempo global para sincronizar a animação da tesoura
        tempoDaRodada = tempoGlobalSugerido;
        return tempoGlobalSugerido;
    }

    void Update()
    {
        if (jogoFinalizado) return;

        elapsedTime += Time.deltaTime;
        
        // Move a tesoura sincronizada com o tempo do GameManagerRework
        float t = elapsedTime / tempoDaRodada;
        if(tesouraTransform != null && startPoint != null && cutPoint != null)
        {
            tesouraTransform.position = Vector3.Lerp(startPoint.position, cutPoint.position, t);
        }
    }

    // Método chamado pelo ShakeController quando a barra chega a 100%
    public void UvaEscapou()
    {
        if (jogoFinalizado) return;
        Vencer(); 
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;
        
        // A tesoura chegou à uva!
        if (uvaController != null) uvaController.TriggerLose();
        base.TempoEsgotado(); // Chama o Perder() interno
    }
}