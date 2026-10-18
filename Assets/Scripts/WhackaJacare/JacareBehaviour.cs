using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JacareBehaviour : MonoBehaviour
{
    public float velocidade = 3f;
    private Transform barco;
    private JacareManager manager;

    [HideInInspector]
    public Sprite spriteOnClick;
    private SpriteRenderer spriteRenderer;
    private bool wasClicked = false;

    // Start is called before the first frame update
    void Start()
    {
        GameObject barcoObj = GameObject.Find("Barco");
        if(barcoObj != null)
        {
            barco = barcoObj.transform;
        }

        manager = FindObjectOfType<JacareManager>();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(barco != null && !wasClicked)
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
                manager.MudarBarcoDerrota();
                manager.Perder();
            }

            Destroy(gameObject);
        }
    }

    private void OnMouseDown()
    {
        //Mudanca para resolver bug com pause
        //Enquanto o jogo estiver pausado, essa funcao nao funciona
        if (!enabled) return;

        if (!wasClicked)
        {
            wasClicked = true;

            if(spriteOnClick != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = spriteOnClick;
            }

            Destroy(gameObject, 0.2f);
        }
        
    }
}
