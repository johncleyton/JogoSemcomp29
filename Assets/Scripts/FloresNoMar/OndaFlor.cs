using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OndaFlor : MonoBehaviour
{
    public Sprite[] ondas;
    public int tipodaOnda;
    private float velocidade;
    private FlorManager florManager;

    // Start is called before the first frame update
    void Start()
    {
        florManager = (FlorManager)FindFirstObjectByType(typeof(FlorManager));
        tipodaOnda = Random.Range(0, ondas.Length);
        while (tipodaOnda == florManager.GetComponent<FlorManager>().tipodaOnda2)
        {
            tipodaOnda = Random.Range(0, ondas.Length);
        }
        GetComponent<SpriteRenderer>().sprite = ondas[tipodaOnda];
        velocidade = Random.Range(2.5f, 3.5f);
        Debug.Log(tipodaOnda);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -8)
        {
            Destroy(this);
            //transform.position = new Vector3(transform.position.x, 7f,transform.position.z);
        }
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.0166f*velocidade, transform.position.z);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Flor")
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            rb.AddForce(new Vector3(0f,-1f,0f), ForceMode2D.Impulse);
        }
    }
}
