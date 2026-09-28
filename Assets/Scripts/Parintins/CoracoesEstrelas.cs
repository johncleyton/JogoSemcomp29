using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoracoesEstrelas : MonoBehaviour
{
    private float timer = 0f;

    [Tooltip("0 para coração (Garantido) e 1 para estrela (Caprichoso)")]
    public int typeOfItem;

    private void OnMouseDown()
    {
        ParintinsManager manager = FindObjectOfType<ParintinsManager>();

        if(manager != null)
        {
            manager.Clicked(typeOfItem);
        }
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > 2f)
        {
            Destroy(gameObject);
        }
    }
}
