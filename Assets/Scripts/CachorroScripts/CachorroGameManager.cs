using UnityEngine;

// Minigame de sobrevivência: o jogador vence se a comida não for comida até o tempo acabar.
// O timer, a barra de tempo e o fim de jogo são controlados pelo GameManagerRework.
public class CachorroGameManager : MinigameBase
{
    [SerializeField] private float _tempoDeSobrevivencia = 5f;
    // Tempo para o jogador perceber a cena antes dos cachorros avançarem
    [SerializeField] private float _delayAntesDoAtaque = 1.25f;

    public float DelayAntesDoAtaque => _delayAntesDoAtaque;

    // Chamado pelo GameManagerRework logo após a cena ser carregada.
    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        DifficultyController.Instance.SpawnarCachorros(faseAtual);

        // O delay entra no tempo da fase, para o jogador ainda ter todo o tempo de sobrevivência após o ataque
        float tempoDeSobrevivencia = _tempoDeSobrevivencia > 0f ? _tempoDeSobrevivencia : tempoGlobalSugerido;
        return _delayAntesDoAtaque + tempoDeSobrevivencia;
    }

    // Sobreviver até o fim do tempo é vencer (se a comida foi comida, Perder() já finalizou o jogo).
    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        Vencer();
    }
}
