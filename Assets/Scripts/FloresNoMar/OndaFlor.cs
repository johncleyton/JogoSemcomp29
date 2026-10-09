using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OndaFlor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -8)
        {
            transform.position = new Vector3(transform.position.x, 7f,transform.position.z);
        }
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.05f, transform.position.z);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Flor")
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            rb.AddForce(new Vector3(0f,-2f,0f), ForceMode2D.Impulse);
        }
    }
}
