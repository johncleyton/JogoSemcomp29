using UnityEngine;

public class PipaMovement : MinigameBase
{

    public float velocity = 300f;
    public Rigidbody2D rb;
    public Animator animator;
    private bool clicou = false;
    private bool fim = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("PipaMovement script has started.");
        Time.timeScale = 1f;
        rb.gravityScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!fim)
        {
            if (Input.GetMouseButtonDown(0) && !PauseBase.mouseOver)
            {
                clicou = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (clicou)
        {
            Debug.Log("Clicou");
            rb.velocity = Vector2.up * velocity * Time.fixedDeltaTime;
            clicou = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        animator.SetTrigger("Colidiu");
        fim = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        Perder();
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        float tempoFixo = 7f;
        return tempoFixo;
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado)
            return;
        Vencer();
    } 
}
