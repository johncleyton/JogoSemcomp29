using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VelaDeteccao : MonoBehaviour
{
    public bool acesa = false;
    public Sprite velaAcesa;
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Colisao com: " + collision.gameObject);
        if (collision.CompareTag("Cursor") && !acesa)
        {
            Light2D[] objetos = GetComponentsInChildren<Light2D>();
            gameObject.GetComponent<SpriteRenderer>().sprite = velaAcesa;
            for (int i = 0; i < objetos.Length; i++)
                objetos[i].enabled = true;
            acesa = true;
        }
    }
}
