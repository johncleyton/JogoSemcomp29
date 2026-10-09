using UnityEngine;

public class FogueiraManager : MinigameBase
{
    public static FogueiraManager Instance;

    [Header("Referências")]
    public MicInput micInput;
    public Transform fogoTransform; 

    [Header("Sprites de Fim de Jogo")]
    public GameObject maoVitoria;
    public GameObject maoDerrota;

    private float fireScale = 0f;
    public float maxFireScale = 3f; 
    
    [Header("Balanceamento")]
    public float blowThreshold = 0.5f; // Volume mínimo para começar a crescer o fogo
    public float crescimentoRate = 2f;
    public float decaimentoRate = 1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Começa com o fogo invisível (escala 0) e as mãos escondidas
        if (fogoTransform != null) fogoTransform.localScale = Vector3.zero;
        if (maoVitoria != null) maoVitoria.SetActive(false);
        if (maoDerrota != null) maoDerrota.SetActive(false);
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // Aumenta a velocidade com que o fogo apaga nas fases mais difíceis
        decaimentoRate = 1f + (faseAtual * 0.5f);
        return tempoGlobalSugerido;
    }

    void Update()
    {
        if (jogoFinalizado) return;

        float volumeAtual = 0f;
        
        // Lê diretamente a variável de volume do seu MicInput
        if (micInput != null) volumeAtual = micInput.loudness;

        // Se o microfone captar sopro/barulho acima do limite
        if (volumeAtual > blowThreshold) 
        {
            // O fogo cresce multiplicando o volume captado pela taxa de crescimento
            fireScale += volumeAtual * crescimentoRate * Time.deltaTime;
        }
        else 
        {
            // Se parar de soprar, o fogo diminui
            fireScale -= decaimentoRate * Time.deltaTime;
        }

        // Limita o tamanho do fogo entre 0 e o tamanho máximo
        fireScale = Mathf.Clamp(fireScale, 0f, maxFireScale);

        if (fogoTransform != null)
            fogoTransform.localScale = new Vector3(fireScale, fireScale, 1f);

        // Se o fogo chegar no tamanho máximo, vence!
        if (fireScale >= maxFireScale)
        {
            VencerFogueira();
        }
    }

    private void VencerFogueira()
    {
        if (jogoFinalizado) return;
        
        // Liga a mão de Like
        if (maoVitoria != null) maoVitoria.SetActive(true);
        Vencer(); 
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;
        
        // Liga a mão de Deslike
        if (maoDerrota != null) maoDerrota.SetActive(true);
        base.TempoEsgotado();
    }
}