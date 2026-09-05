using UnityEngine;
using UnityEngine.UI;

public enum EstadoOvelha { MuitoNova, EmPerfeitoEstado, MuitoVelha }

public class OvelhaController : MonoBehaviour
{
    [Header("Estado")]
    public EstadoOvelha estadoAtual;

    [Header("Visual - preencha o campo correspondente ao tipo do seu objeto")]
    [Tooltip("Caso comum: a Ovelha é um prefab de mundo com Sprite Renderer no root (seu caso atual).")]
    public SpriteRenderer spriteRenderer;
    [Tooltip("Só use se a ovelha for uma Image de UI dentro de um Canvas.")]
    public Image imagemUI;

    [Header("Sprites por estado")]
    public Sprite spriteMuitoNova;
    public Sprite spriteEmPerfeitoEstado;
    public Sprite spriteMuitoVelha;

    [Header("Opcional")]
    public Animator animator;

    void Awake()
    {
        // Facilita: se esqueceu de arrastar no prefab, pega o SpriteRenderer do próprio root.
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Inicializar(EstadoOvelha estado)
    {
        estadoAtual = estado;
        AtualizarVisual();
    }

    void AtualizarVisual()
    {
        Sprite spriteCorreto = ObterSpriteDoEstado(estadoAtual);
        if (spriteCorreto == null)
        {
            Debug.LogWarning($"OvelhaController: nenhum sprite atribuído para o estado {estadoAtual}. " +
                              "Arraste as 3 imagens no prefab da Ovelha (spriteMuitoNova / spriteEmPerfeitoEstado / spriteMuitoVelha).");
            return;
        }

        if (spriteRenderer != null) spriteRenderer.sprite = spriteCorreto;
        if (imagemUI != null) imagemUI.sprite = spriteCorreto;
    }

    Sprite ObterSpriteDoEstado(EstadoOvelha estado)
    {
        switch (estado)
        {
            case EstadoOvelha.MuitoNova: return spriteMuitoNova;
            case EstadoOvelha.EmPerfeitoEstado: return spriteEmPerfeitoEstado;
            case EstadoOvelha.MuitoVelha: return spriteMuitoVelha;
            default: return null;
        }
    }

    // Agora cada ovelha é instanciada por rodada (fila), então ela pode
    // se destruir de verdade ao sair da tela.
    public void SairDaTela()
    {
        if (animator != null) animator.SetTrigger("SairDaTela");
        Destroy(gameObject, 0.5f);
    }
}