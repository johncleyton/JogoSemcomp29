using UnityEngine;

public class CursorCustom : MonoBehaviour
{

    public Texture2D cursorMao;

    // (0,0)
    public Vector2 hotSpot = Vector2.zero;

    void Start()
    {
        // CORRIGIDO: evita sobrescrever o cursor com "nada" caso a textura
        // não tenha sido arrastada no Inspector; agora avisa no Console.
        if (cursorMao != null)
        {
            Cursor.SetCursor(cursorMao, hotSpot, CursorMode.Auto);
        }
        else
        {
            Debug.LogWarning("CursorCustom: nenhuma textura foi atribuída em 'cursorMao'.");
        }
    }
}
