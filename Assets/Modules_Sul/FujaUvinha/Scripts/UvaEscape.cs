using UnityEngine;

public class UvaEscape : MonoBehaviour
{
    public float forcaNecessaria = 2.5f; 
    public float progressoAtual = 0f;
    public float objetivoParaEscapar = 100f; 
    public float taxaDeDecaimento = 10f; 

    void Update()
    {
        Vector3 aceleracao = Input.acceleration;
        
        float intensidadeAgito = aceleracao.sqrMagnitude;

        if (intensidadeAgito > forcaNecessaria)
        {
            // O jogador está chacoalhando! Aumenta o progresso.
            progressoAtual += intensidadeAgito * Time.deltaTime * 20f;
            
            // UVA TREMENDO...
        }
        else
        {
            // Se parar de bater, o progresso cai (aumentando a tensão)
            progressoAtual -= taxaDeDecaimento * Time.deltaTime;
        }

        // Mantém o progresso entre 0 e o objetivo
        progressoAtual = Mathf.Clamp(progressoAtual, 0, objetivoParaEscapar);

        if (progressoAtual >= objetivoParaEscapar)
        {
            CairDoCacho();
        }
    }

    void CairDoCacho()
    {
        // Desativa a lógica de shake
        this.enabled = false;
        
        // Ativa a gravidade do Rigidbody2D para ela cair
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if(rb != null) rb.gravityScale = 1f;

        // Carrega a próxima cena ou inicia a próxima fase!
        Debug.Log("A Uvinha se libertou!");
    }
}