using UnityEngine;

// Minigame de sobrevivência: o jogador vence se a comida não for comida até o tempo acabar.
// O timer, a barra de tempo e o fim de jogo são controlados pelo GameManagerRework.
public class CachorroGameManager : MinigameBase
{
    [SerializeField] private float _tempoDeSobrevivencia = 5f;

    // Chamado pelo GameManagerRework logo após a cena ser carregada.
    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        DifficultyController.Instance.SpawnarCachorros(faseAtual);
        return _tempoDeSobrevivencia > 0f ? _tempoDeSobrevivencia : tempoGlobalSugerido;
    }

    // Sobreviver até o fim do tempo é vencer (se a comida foi comida, Perder() já finalizou o jogo).
    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        Vencer();
    }
}
