using System.Collections;
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

    public void VencerComAtraso(float tempoDeEspera)
    {
        if (jogoFinalizado) 
            return;
        jogoFinalizado = true;
        
        // Pausa o relógio do GameManager para a cena não fechar
        GameManagerRework.Instance.timerCongelado = true; 
        StartCoroutine(RotinaFimDeJogo(true, tempoDeEspera));
    }

    public void PerderComAtraso(float tempoDeEspera)
    {
        if (jogoFinalizado) 
            return;
        jogoFinalizado = true;

        GameManagerRework.Instance.timerCongelado = true; 
        StartCoroutine(RotinaFimDeJogo(false, tempoDeEspera));
    }

    private IEnumerator RotinaFimDeJogo(bool vitoria, float tempo)
    {
        // Aguarda a duração da animacao
        yield return new WaitForSeconds(tempo); 
        
        // Depois de esperar, chama a função correta
        if (vitoria)
        {
            Debug.Log("Minigame: Vitória após animação!");
            GameManagerRework.Instance.VenceuMinigame();
        }
        else
        {
            Debug.Log("Minigame: Derrota após animação!");
            GameManagerRework.Instance.GameOver();
        }
    }

    /*
    public override void TempoEsgotado()
    {
        if (jogoFinalizado) 
            return; 
        anim.SetTrigger("tempoAcabou");
        PerderComAtraso(2f);
    }
    */
}