using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarroControle : MinigameBase
{
    public Animator animator;
    public float laneDistance = 3f;
    public float moveSpeed = 10f;

    public float rotationSpeed = 10f; 

    private int currentLane = 1; 

   void Update()
   {
        if (jogoFinalizado) return;

        if (Input.GetMouseButtonDown(0))
        {
            float mouseX = Input.mousePosition.x;

            if (mouseX < Screen.width / 2)
            {
                currentLane--;
            }
            else
            {
                currentLane++;
            }

            currentLane = Mathf.Clamp(currentLane, 0, 2);
        }

        float targetX = (currentLane - 1) * laneDistance;

        Vector3 targetPosition = new Vector3(
            targetX,
            transform.position.y,
            0
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        Rotate(targetX);
    }

    private void Rotate(float targetX)
    {
        float deltaX = targetX - transform.position.x;

        float targetAngle = Mathf.Clamp(-deltaX * 5f, -20, 20);
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Buraco") || other.GetComponent<Buraco>() != null)
        {
            animator.SetTrigger("explodir");
            PerderComAtraso(0.8f);
        }
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) 
            return;
        VencerComAtraso(1f);
    }
}
