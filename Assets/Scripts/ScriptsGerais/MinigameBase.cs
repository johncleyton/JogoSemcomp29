using UnityEngine;

public abstract class MinigameBase : MonoBehaviour
{
    protected bool jogoFinalizado = false; 

    // Por padrão, apenas usa o timerAtual, porem por meio de um override da pra mudar as regras
    public virtual float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        return tempoGlobalSugerido; 
    }

    // Exemplo de override pra vcs usarem
    /*public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        // O tempo que voces vao usar
        float tempoFixo = 10f; 
        // As contas de como voces vao deixar mais dificil
        quantidadeDeInimigos = 3 + (faseAtual / 5);

        return tempoFixo;
    }*/

    // Método padronizado para Vitória
    public void Vencer()
    {
        if (jogoFinalizado) 
            return; // Impede que ganhe duas vezes
        jogoFinalizado = true;
        
        Debug.Log("Minigame: Vitória!");
        GameManagerRework.Instance.VenceuMinigame(); 
    }

    // Método padronizado para Derrota
    public void Perder()
    {
        if (jogoFinalizado) 
            return; // Impede que perca duas vezes
        jogoFinalizado = true;

        Debug.Log("Minigame: Derrota!");
        GameManagerRework.Instance.GameOver(); 
    }

    // Se acaba o tempo perde, porém da pra usar override pra mudar isso
    public virtual void TempoEsgotado()
    {
        if (jogoFinalizado) 
            return;
        Perder();
    }


    /* Exemplo de override
    public override void TempoEsgotado()
    {
        if (jogoFinalizado) 
            return;
        Vencer();
    }
    */
}