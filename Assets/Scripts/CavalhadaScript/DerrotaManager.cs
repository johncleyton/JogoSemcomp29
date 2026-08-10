using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DerrotaManager : MonoBehaviour
{
    private MinigameBase chefeDaFase;

    void Start()
    {
        chefeDaFase = FindObjectOfType<MinigameBase>();
        if (chefeDaFase == null)
            Debug.LogError("DerrotaManager não encontrou o MinigameBase na cena!");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Checa se o objeto que bateu é da Layer 7
        if (collision.gameObject.layer == 7)
        {
            if (chefeDaFase != null)
                chefeDaFase.Perder(); 
        }
    }
}