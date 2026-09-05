using UnityEngine;

// Nome do arquivo agora bate com o nome da classe (evita confusão no Editor).
// Substitua o antigo TosquiadoraManager.cs por este arquivo.
public class TosquiadoraController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float velocidadeHorizontal = 6f;
    public float velocidadeQueda = 20f;
    public float limiteEsquerdo = -2.5f;
    public float limiteDireito = 2.5f;
    public float alturaInicial = 3f;
    public float limiteErroY = -4f;

    // Controlado pelo TosquieManager: só anda/cai durante a fase de tosquia
    // (isto é, depois que o jogador acertou o pente correto).
    [HideInInspector] public bool PodeAgir = false;

    private bool caindo = false;
    private int direcao = 1;

    void Update()
    {
        if (!PodeAgir) return;
        if (TosquieManager.Instance != null && TosquieManager.Instance.JogoEncerrado) return;

        if (!caindo)
        {
            transform.Translate(Vector2.right * direcao * velocidadeHorizontal * Time.deltaTime);

            if (transform.position.x >= limiteDireito) direcao = -1;
            if (transform.position.x <= limiteEsquerdo) direcao = 1;

            if (Input.GetMouseButtonDown(0))
            {
                caindo = true;
            }
        }
        else
        {
            transform.Translate(Vector2.down * velocidadeQueda * Time.deltaTime);

            if (transform.position.y <= limiteErroY)
            {
                RegistrarFalha();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (!PodeAgir) return;

        if (caindo && outro.CompareTag("La"))
        {
            Destroy(outro.gameObject);
            if (TosquieManager.Instance != null) TosquieManager.Instance.CortouLa();
            ResetarPosicao();
        }
    }

    void RegistrarFalha()
    {
        if (TosquieManager.Instance != null) TosquieManager.Instance.TentativaFalhou();
        ResetarPosicao();
    }

    void ResetarPosicao()
    {
        transform.position = new Vector3(transform.position.x, alturaInicial, transform.position.z);
        caindo = false;
    }

    // Chamado pelo TosquieManager no começo de cada rodada de tosquia
    public void ResetarParaTopo()
    {
        transform.position = new Vector3(0f, alturaInicial, transform.position.z);
        caindo = false;
        direcao = 1;
    }
}