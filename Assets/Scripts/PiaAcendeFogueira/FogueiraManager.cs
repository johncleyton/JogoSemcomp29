using UnityEngine;

public class FogueiraManager : MinigameBase
{
    [Header("Referências")]
    public MicInput micInput;
    public Transform fogoSprite;
    
    [Header("Balanceamento Base")]
    public float blowThreshold = 0.5f;
    public float fireGrowthRate = 25f;
    public float fireDecayRate = 10f; 
    public float maxFireScale = 3f;
    
    private float currentFire = 0f;

    // --- INTEGRAÇÃO COM O NOVO CORE ---
    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // Quanto mais avançada a fase, mais rápido a fogueira apaga sem sopro!
        fireDecayRate = 10f + (faseAtual * 2f);
        return tempoGlobalSugerido;
    }

    void Start()
    {
        if (fogoSprite != null)
            fogoSprite.localScale = Vector3.one * 0.1f;
    }

    void Update()
    {
        if (jogoFinalizado) return; // Trava de segurança

        // Evita erros caso os objetos não tenham sido arrastados no Inspector
        if (micInput == null || fogoSprite == null) return;

        if (micInput.loudness > blowThreshold)
            currentFire += fireGrowthRate * Time.deltaTime;
        else
            currentFire -= fireDecayRate * Time.deltaTime;

        currentFire = Mathf.Clamp(currentFire, 0f, 100f);
        
        float mappedScale = Mathf.Lerp(0.1f, maxFireScale, currentFire / 100f);
        fogoSprite.localScale = Vector3.one * mappedScale;

        if (currentFire >= 100f)
        {
            Debug.Log("Fogueira acesa! O pai tá orgulhoso!");
            Vencer();
        }
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return; // Trava de segurança
        
        Debug.Log("Demorou demais, piá!");
        base.TempoEsgotado(); 
    }
}