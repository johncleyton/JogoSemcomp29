using UnityEngine;

public class CameraPanMobile : MonoBehaviour
{
    private Vector3 dragOrigin;

    void Update()
    {
        // Funciona tanto para mouse (teste no editor) quanto para touch único no celular
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += difference;
        }
    }
}