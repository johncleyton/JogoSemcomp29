using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotaManager : MonoBehaviour
{
    public float velocidade = 20f;
    public bool estaNaZonaDeAcerto = false;

    void Update()
    {
        transform.position += Vector3.left * velocidade * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("marcador")) estaNaZonaDeAcerto = true;
        if (other.CompareTag("sumidouro")) 
        {
            Debug.Log("Perdeu");
            FindObjectOfType<MinigameBase>().Perder();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("marcador")) estaNaZonaDeAcerto = false;
    }

    public void Acertar()
    {
        GetComponent<SpriteRenderer>().color = Color.green;
        Destroy(gameObject, 0.5f);
    }
}