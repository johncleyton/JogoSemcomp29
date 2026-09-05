using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JacareBehaviour : MonoBehaviour
{
    public float velocidade = 3f;
    private Transform barco;
    private MinigameBase manager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject barcoObj = GameObject.Find("Barco");
        if(barcoObj != null)
        {
            barco = barcoObj.transform;
        }

        manager = FindObjectOfType<JacareManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(barco != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, barco.position, velocidade * Time.deltaTime);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.name == "Barco")
        {
            if(manager != null)
            {
                manager.Perder();
            }

            Destroy(gameObject);
        }
    }

    private void OnMouseDown()
    {
        Destroy(gameObject);
    }
}
