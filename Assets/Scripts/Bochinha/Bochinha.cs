using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bochinha : MonoBehaviour
{

    private GameObject bochaPrefab;
    private Transform launchPoint;

    private float maxForce = 25;
    private float forceMultiplier = 2f;

    private Vector3 dragStartPos;
    private bool isDragging = false;
    private LineRenderer aimLine;
    // Start is called before the first frame update
    void Start()
    {
        aimLine = GetComponent<LineRenderer>();
        if(aimLine) aimLine.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartPos = Input.mousePosition;

            if(aimLine) aimLine.positionCount = 2;
        }

        if (isDragging)
        {
            Vector3 currentMousePos = Input.mousePosition;

            // inversa para estilingue
            Vector3 dragVector = dragStartPos - currentMousePos;

            if (aimLine)
            {
                aimLine.SetPosition(0, launchPoint.position);
                Vector3 targetDir = new Vector3(dragVector.x, 0, dragVector.y);

                float forceMag = Mathf.Min(dragVector.magnitude * 0.05f, maxForce);

                // literal A + AB, ponto inicial + vector ate final do ponto inicial * força
                aimLine.SetPosition(1, launchPoint.position + targetDir*(forceMag*0.3f));
            }

        }
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if(aimLine) aimLine.positionCount = 0;

            Vector3 dragVector = dragStartPos - Input.mousePosition;
            Vector3 launchDirection = new Vector3(dragVector.x, 0, dragVector.y).normalized;
            float launchForce = Mathf.Min(dragVector.magnitude * forceMultiplier, maxForce);

            LaunchBocha(launchDirection, launchForce);
        }
    }

    private void LaunchBocha(Vector3 direction, float force)
    {
        GameObject newBocha = Instantiate(bochaPrefab, launchPoint.position, Quaternion.identity);
        Rigidbody rb = newBocha.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }
}