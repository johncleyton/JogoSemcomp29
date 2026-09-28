using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarroControle : MinigameBase
{
    public Animator animator;
    public float laneDistance = 3f;
    public float moveSpeed = 10f;

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
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Buraco") || other.GetComponent<Buraco>() != null)
        {
            animator.SetTrigger("explodir");
            PerderComAtraso(2f);
        }
    }

    public override void TempoEsgotado()
    {
        if (jogoFinalizado) 
            return;
        VencerComAtraso(2f);
    }
}
