using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mosquito : MonoBehaviour
{
    public float speed = 15f;
    public float pushForce = 40f;
    public float drag = 20f;            

    private Transform centerTarget;
    private Vector2 pushVelocity;       

    public void Initialize(Transform target)
    {
        centerTarget = target;
    }

    void Update()
    {
        if (centerTarget == null) return;

        if (pushVelocity.magnitude > 0.01f)
        {
            transform.position += (Vector3)pushVelocity * Time.deltaTime;
            pushVelocity = Vector2.MoveTowards(pushVelocity, Vector2.zero, drag * Time.deltaTime);
        }
        else
        {
            pushVelocity = Vector2.zero;
            transform.position = Vector3.MoveTowards(transform.position, centerTarget.position, speed * Time.deltaTime);
        }
    }

    private void OnMouseDown()
    {
        if (centerTarget == null) return;

        Vector2 awayFromPlayer = ((Vector2)transform.position - (Vector2)centerTarget.position).normalized;
        pushVelocity = awayFromPlayer * pushForce;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Aassasas");
            FindObjectOfType<EspantarMosquitos>()?.PerdeuJogo();
            Destroy(gameObject);
        }
    }
    
}
