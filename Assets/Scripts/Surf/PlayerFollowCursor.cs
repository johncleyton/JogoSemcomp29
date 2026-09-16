using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFollowCursorPhysics : MinigameBase
{
    [Header("Configuracoes de Movimento Vertical (Y)")]
    public float smoothTime = 0.15f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    [Header("Mecanica de Avanco (X) por Oscilacao")]
    public float impulseForce = 0.5f;
    public float maxForwardSpeed = 5f;
    public float forwardDecay = 2f;
    public float oscillationThreshold = 0.05f;

    [Header("Mecanica de Recuo (Puxar para tras)")]
    public float backwardPushSpeed = 1.5f;
    public float minX = -6.0f;
    public float maxX = 8f;
    public float deathThresholdX = -5.5f;

    [Header("Morte")]
    public float tempoAnimacaoMorte = 1.2f;

    [Header("Vitória")]
    public float tempoAnimacaoVitoria = 1f;

    [Header("Animator")]
    public string paramCima = "cima";
    public string paramBaixo = "baixo";
    public float verticalStateCooldown = 0.12f;
    private Animator animator;

    private Camera mainCamera;
    private Rigidbody2D rb;
    private float velocityY = 0f;
    private float currentForwardSpeed = 0f;
    private float lastTargetY = 0f;
    private bool movingUp = false;
    private bool hasInput = false;
    private Vector2 targetWorldPos;
    private bool isDead = false;

    private bool? currentVerticalState = null;
    private float verticalStateTimer = 0f;

    [Header("Knockback")]
    public float knockbackDecay = 8f;
    private float knockbackVelocity = 0f;

    [Header("Spawner de Galhos")]
    public SpawnGalhos spawnGalhos;

    [Header("Aceleracao geral do jogo")]
    public float spawnTimerReducaoPorFase = 0.05f;
    public float spawnTimerMinimo = 0.5f;

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        if (spawnGalhos != null)
        {
            float novoSpawnTimer = Mathf.Max(
                spawnTimerMinimo,
                spawnGalhos.baseSpawnTimer - (faseAtual * spawnTimerReducaoPorFase)
            );
            spawnGalhos.SetSpawnTimer(novoSpawnTimer);
        }

        return tempoGlobalSugerido;
    }

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        lastTargetY = transform.position.y;
        targetWorldPos = transform.position;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (isDead) return;

        verticalStateTimer += Time.deltaTime;

        Vector3 inputPosition = Vector3.zero;
        hasInput = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPosition = touch.position;
            hasInput = true;
        }
        else if (Input.GetMouseButton(0))
        {
            inputPosition = Input.mousePosition;
            hasInput = true;
        }

        bool? desiredState = null;

        if (hasInput)
        {
            inputPosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
            targetWorldPos = worldPos;

            float currentTargetY = worldPos.y;
            float deltaY = currentTargetY - lastTargetY;

            if (Mathf.Abs(deltaY) > oscillationThreshold)
            {
                bool dynamicMovingUp = deltaY > 0;
                if (dynamicMovingUp != movingUp)
                {
                    movingUp = dynamicMovingUp;
                    currentForwardSpeed += impulseForce;
                    currentForwardSpeed = Mathf.Clamp(currentForwardSpeed, 0f, maxForwardSpeed);
                }

                desiredState = dynamicMovingUp;
            }

            lastTargetY = currentTargetY;
        }

        RequestVerticalState(desiredState);
    }

    private void RequestVerticalState(bool? desiredState)
    {
        if (desiredState == currentVerticalState)
        {
            return;
        }

        if (verticalStateTimer < verticalStateCooldown)
        {
            return;
        }

        currentVerticalState = desiredState;
        verticalStateTimer = 0f;

        ApplyVerticalAnimatorBools(desiredState);
    }

    private void ApplyVerticalAnimatorBools(bool? isMovingUp)
    {
        if (animator == null) return;

        bool cima = isMovingUp == true;
        bool baixo = isMovingUp == false && isMovingUp != null;

        if (animator.GetBool(paramCima) != cima)
            animator.SetBool(paramCima, cima);

        if (animator.GetBool(paramBaixo) != baixo)
            animator.SetBool(paramBaixo, baixo);
    }

    void FixedUpdate()
    {
        if (isDead) return;

        currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, 0f, forwardDecay * Time.fixedDeltaTime);
        knockbackVelocity = Mathf.MoveTowards(knockbackVelocity, 0f, knockbackDecay * Time.fixedDeltaTime);

        float movementX = (currentForwardSpeed - backwardPushSpeed + knockbackVelocity) * Time.fixedDeltaTime;
        float newX = rb.position.x + movementX;
        newX = Mathf.Clamp(newX, minX, maxX);

        float newY = rb.position.y;
        if (hasInput)
        {
            float targetYClamped = Mathf.Clamp(targetWorldPos.y, minY, maxY);
            newY = Mathf.SmoothDamp(rb.position.y, targetYClamped, ref velocityY, smoothTime);
        }

        rb.MovePosition(new Vector2(newX, newY));

        if (newX <= deathThresholdX)
        {
            Die();
        }
    }

    public void ApplyKnockback(float amount)
    {
        knockbackVelocity -= Mathf.Abs(amount);
    }

    private void Die()
    {
        if (jogoFinalizado) return;

        isDead = true;
        animator.SetTrigger("Cair");
        rb.velocity = Vector2.zero;

        StartCoroutine(RotinaMorteSlowMotion());
    }

    private IEnumerator RotinaMorteSlowMotion()
    {
        float tempoDecorrido = 0f;

        while (tempoDecorrido < tempoAnimacaoMorte)
        {
            tempoDecorrido += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(1f, 0f, tempoDecorrido / tempoAnimacaoMorte);
            yield return null;
        }

        Time.timeScale = 1f;
        PerderComAtraso(0f);
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) return;

        isDead = true;
        VencerComAtraso(tempoAnimacaoVitoria);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}