using UnityEngine;

public class PlayerFollowCursor : MonoBehaviour
{
    [Header("Configurações de Movimento Vertical (Y)")]
    public float smoothTime = 0.15f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    [Header("Mecânica de Avanço (X) por Oscilação")]
    public float impulseForce = 0.5f;
    public float maxForwardSpeed = 5f;
    public float forwardDecay = 2f;
    public float oscillationThreshold = 0.05f;

    [Header("Mecânica de Recuo (Puxar para trás)")]
    public float backwardPushSpeed = 1.5f;
    public float minX = -8f;
    public float maxX = 8f;

    private Camera mainCamera;
    private Vector3 velocityY = Vector3.zero;

    private float currentForwardSpeed = 0f;
    private float lastTargetY = 0f;
    private bool movingUp = false;

    void Start()
    {
        mainCamera = Camera.main;
        lastTargetY = transform.position.y;
    }

    void Update()
    {
        Vector3 inputPosition = Vector3.zero;
        bool hasInput = false;

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
            Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(inputPosition);

            float currentTargetY = targetWorldPos.y;
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

            Vector3 targetPosition = new Vector3(transform.position.x, targetWorldPos.y, transform.position.z);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

            float newY = Vector3.SmoothDamp(transform.position, targetPosition, ref velocityY, smoothTime).y;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, 0f, forwardDecay * Time.deltaTime);

        float movementX = (currentForwardSpeed - backwardPushSpeed) * Time.deltaTime;

        float newX = transform.position.x + movementX;
        newX = Mathf.Clamp(newX, minX, maxX);

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}