using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Galho : MonoBehaviour
{
    public float speed;

    [Header("Destruicao")]
    public float destroyXPosition = -11f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(speed, 0);
    }

    private void Update()
    {
        if (rb.position.x < destroyXPosition)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(speed, rb.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerFollowCursorPhysics player = collision.gameObject.GetComponent<PlayerFollowCursorPhysics>();
        if (player != null)
        {
            player.MorrerPorObstaculo();
        }
    }
}