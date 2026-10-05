using System.Collections;
using UnityEngine;

public class SaciBoss : MinigameBase
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer peneira;

    public SpriteRenderer jogador;           // o jogador na cena
    public Sprite spriteJogandoPeneira;      // imagem do jogador arremessando

    public float tempoDeAviso = 0.8f;
    public float tempoDeParry = 1f;
    public float velocidadePeneira = 15f;

    private bool janelaDeParry = false;
    private bool jogandoPeneira = false;
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
        // A peneira voa mesmo depois do fim do minigame
        if (jogandoPeneira)
        {
            peneira.transform.position = Vector3.MoveTowards(
                peneira.transform.position,
                spriteRenderer.transform.position,
                velocidadePeneira * Time.deltaTime);
        }

        if (acabou) return;

        if (Input.GetMouseButtonDown(0))
        {
            acabou = true;
            StopCoroutine(saciRoutine);

            if (janelaDeParry)
            {
                janelaDeParry = false;
                Debug.Log("acertou!");

                jogador.sprite = spriteJogandoPeneira;

                peneira.gameObject.SetActive(true);
                jogandoPeneira = true;

                VencerComAtraso(1.5f);
            }
            else
            {
                Perder();
            }
        }
    }

    IEnumerator SaciCoroutine()
    {
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Loop"));

        yield return new WaitForSeconds(Random.Range(2f, 5f));

        animator.SetTrigger("Preparar");
        yield return new WaitForSeconds(tempoDeAviso);

        animator.SetTrigger("Final");
        janelaDeParry = true;
        yield return new WaitForSeconds(tempoDeParry);

        janelaDeParry = false;
        acabou = true;
        Perder();
    }
}