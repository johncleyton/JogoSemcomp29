using System;
using UnityEngine;

public class PersonMovement : MonoBehaviour
{
    [Header("Mode Selection")]
    [Tooltip("Check this to use custom Jump Height & Distance instead of manual gravity/speed.")]
    [SerializeField] private bool useArcSettings = true;

    [Header("Movement")]
    [SerializeField] private float runSpeed = 20f;

    [Header("Normal Jump Settings")]
    [SerializeField] private float gravity = 25f;
    [SerializeField] private float jumpSpeed = 8f;

    [Header("Arc Jump Settings")]
    [SerializeField] private float jumpHeight = 3.5f;     // Altura Máxima
    [SerializeField] private float jumpDistance = 6f;     // Distância horizontal

    [Header("Controller")]
    [SerializeField] private PulaFogueiraController gameController;

    private float groundCoordinates;
    private bool isGrounded = true;

    private float verticalSpeed;
    private float calculatedGravity;
    private float calculatedJumpSpeed;

    void Start()
    {
        groundCoordinates = transform.position.y;
        RecalculateArcParameters();

        transform.position = new Vector3(-runSpeed * (gameController.getMinigameDuration() - 2.5f), transform.position.y, 0);
    }

    void OnValidate()
    {
        RecalculateArcParameters();
    }

    private void RecalculateArcParameters()
    {
        if (useArcSettings)
        {
            float totalAirTime = jumpDistance / runSpeed;
            float timeToApex = totalAirTime / 2f;

            // Formulas Usadas:
            // g = 2 * h / (t_apex^2)
            // v0 = g * t_apex
            calculatedGravity = (2f * jumpHeight) / Mathf.Pow(timeToApex, 2);
            calculatedJumpSpeed = calculatedGravity * timeToApex;
        }
        else
        {
            calculatedGravity = gravity;
            calculatedJumpSpeed = jumpSpeed;
        }
    }

    void Update()
    {
        // Velocidaed vertical e checagem do chão
        if (!isGrounded)
        {
            verticalSpeed -= calculatedGravity * Time.deltaTime;

            if (transform.position.y < groundCoordinates)
            {
                isGrounded = true;
                verticalSpeed = 0f;
                transform.position = new Vector3(transform.position.x, groundCoordinates, 0f);
            }
        }

        transform.position += new Vector3(runSpeed, verticalSpeed, 0f) * Time.deltaTime;

        // Checa o input
        if (Input.GetMouseButtonDown(0) && isGrounded)
        {
            isGrounded = false;
            verticalSpeed = calculatedJumpSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        gameController.playerHit();
    }
}