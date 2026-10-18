using System.Collections;
using UnityEngine;

public class SaciBoss : MinigameBase
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer peneira;

    public SpriteRenderer jogador;           // o jogador na cena
    public Sprite spriteJogandoPeneira;      // imagem do jogador arremessando

    public float velocidadePeneira = 15f;
    [Range(0.5f, 1f)]
    public float escalaFinalPeneira = 0.85f; // quanto ela encolhe até chegar no Saci (1 = não encolhe)

    private bool janelaDeParry = false;
    private bool acabou = false;
    private Coroutine saciRoutine;

    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        saciRoutine = StartCoroutine(SaciCoroutine());
    }

    void Update()
    {
        if (acabou) return;
        if (!Input.GetMouseButtonDown(0)) return;

        acabou = true;
        StopCoroutine(saciRoutine);

        jogador.sprite = spriteJogandoPeneira;
        peneira.gameObject.SetActive(true);

        if (janelaDeParry)
        {
            janelaDeParry = false;
            Debug.Log("acertou!");
            StartCoroutine(PeneiraAcerto());
        }
        else
        {
            StartCoroutine(PeneiraErro());
        }
    }

    // Acerto: a peneira vai até o Saci encolhendo de leve (profundidade)
    IEnumerator PeneiraAcerto()
    {
        yield return MoverPeneira(spriteRenderer.transform.position, escalaFinalPeneira);
        VencerComAtraso(1.5f);
        animator.SetTrigger("Final");
    }

    // Erro: a peneira bate no Saci e é defletida de volta ao jogador
    IEnumerator PeneiraErro()
    {
        yield return MoverPeneira(spriteRenderer.transform.position, 1f);
        yield return MoverPeneira(jogador.transform.position, 1f);
        Perder();
    }

    // Move a peneira até o destino (mantendo o Z dela) e interpola a escala pelo progresso do trajeto
    IEnumerator MoverPeneira(Vector3 destino, float multiplicadorEscala)
    {
        Transform t = peneira.transform;
        Vector3 inicio = t.position;
        Vector3 alvo = new Vector3(destino.x, destino.y, inicio.z);

        Vector3 escalaInicial = t.localScale;
        Vector3 escalaAlvo = escalaInicial * multiplicadorEscala;
        float distanciaTotal = Vector3.Distance(inicio, alvo);

        while (t.position != alvo)
        {
            t.position = Vector3.MoveTowards(t.position, alvo, velocidadePeneira * Time.deltaTime);

            float progresso = distanciaTotal > 0f
                ? 1f - Vector3.Distance(t.position, alvo) / distanciaTotal
                : 1f;
            t.localScale = Vector3.Lerp(escalaInicial, escalaAlvo, progresso);

            yield return null;
        }
    }

    IEnumerator SaciCoroutine()
    {
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Loop"));

        yield return new WaitForSeconds(Random.Range(1f, 3.5f));

        animator.SetTrigger("Preparar");

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("JanelaDeAcerto"));
        janelaDeParry = true;

        yield return new WaitWhile(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("JanelaDeAcerto"));

        janelaDeParry = false;
        acabou = true;
        Perder();
    }
}