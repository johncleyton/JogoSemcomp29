using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Galho : MonoBehaviour
{
    public float speed;

    [Header("Impacto no Player")]
    public float knockbackDistance = 2f;

    [Header("Destruicao")]
    public float destroyXPosition = -11f;
    public float colliderDisableTime = 0.5f;

    private Rigidbody2D rb;
    private Collider2D col;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerFollowCursorPhysics player = collision.gameObject.GetComponent<PlayerFollowCursorPhysics>();
        if (player != null)
        {
            player.ApplyKnockback(knockbackDistance);
        }

        StartCoroutine(DisableColliderTemporarily());
    }

    private IEnumerator DisableColliderTemporarily()
    {
        col.enabled = false;
        yield return new WaitForSeconds(colliderDisableTime);
        col.enabled = true;
    }
}