using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDropGrav : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    private bool isClicked = false;
    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isClicked)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = -4.92f;
            Debug.Log(mouseWorldPos);
            transform.position = mouseWorldPos;

        }
    }

    private void OnMouseDown()
    {
        isClicked = true;
        rb.gravityScale = 0f;
    }

    private void OnMouseUp()
    {
        isClicked = false;
        rb.gravityScale = 1f;
        rb.velocity = Vector3.zero;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 4)
        {
            Destroy(gameObject);
        }
    }
}
