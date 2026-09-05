using UnityEngine;

public class PipaMovement : MinigameBase
{

    public float velocity = 300f;
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public Color corDeColisao = Color.yellow;
    private bool clicou = false;

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
        if (Input.GetMouseButtonDown(0))
        {
            clicou = true;
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
        sprite.color = corDeColisao;
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
