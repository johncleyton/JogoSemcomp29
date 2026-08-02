using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedacoLa : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Tosquiadora"))
        {
            CortarLa();
        }
    }

    private void CortarLa()
    {
        // Aqui poderás instanciar um efeito de partículas de lã a voar
        Debug.Log("Pedaço de lã cortado!");
        
        // Avisa o Manager que um pedaço foi cortado (para contar a vitória)
        // TosquieManager.Instance.RegistrarLãCortada(); 
        
        // Desativa o objeto da lã
        gameObject.SetActive(false);
    }
}
