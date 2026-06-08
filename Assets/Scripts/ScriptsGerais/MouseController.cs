using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public Texture2D texturaCursor;
    public GameObject cursor, objetoCursor;
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(cursor);
        if (cursor != null)
        {
            objetoCursor = Instantiate(cursor);
            Debug.Log("Cursor instanciado");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (objetoCursor != null)
        {
            objetoCursor.transform.position = mousePos;
            //Debug.Log("Movimentando cursor para: " + objetoCursor.transform.position);
        }
    }
}
