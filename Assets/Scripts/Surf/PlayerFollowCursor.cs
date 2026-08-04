using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFollowCursorPhysics : MonoBehaviour
{
    [Header("Configuracoes de Movimento Vertical (Y)")]
    public float smoothTime = 0.15f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    [Header("Mecanica de Avan�o (X) por Oscilacao")]
    public float impulseForce = 0.5f;
    public float maxForwardSpeed = 5f;
    public float forwardDecay = 2f;
    public float oscillationThreshold = 0.05f;

    [Header("Mecanica de Recuo (Puxar para tras)")]
    public float backwardPushSpeed = 1.5f;
    public float minX = -8f;
    public float maxX = 8f;

    private Camera mainCamera;
    private Rigidbody2D rb;
    private float velocityY = 0f;

    private float currentForwardSpeed = 0f;
    private float lastTargetY = 0f;
    private bool movingUp = false;

    private bool hasInput = false;
    private Vector2 targetWorldPos;

    private bool isDead = false;

    [Header("Knockback")]
    public float knockbackDecay = 8f;

    private float knockbackVelocity = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        lastTargetY = transform.position.y;
        targetWorldPos = transform.position;
    }

    void Update()
    {
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
            }
            lastTargetY = currentTargetY;
        }
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

        if (newX <= minX)
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
        isDead = true;
        Debug.Log("Player morreu: chegou no X minimo do surf.");
    }
}