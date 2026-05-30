using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorCustom : MonoBehaviour
{

    public Texture2D cursorMao;

    // (0,0)
    public Vector2 hotSpot = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.SetCursor(cursorMao, hotSpot, CursorMode.Auto);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
