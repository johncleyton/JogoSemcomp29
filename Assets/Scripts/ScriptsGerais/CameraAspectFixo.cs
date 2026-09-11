using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraAspectFixo : MonoBehaviour
{
    [Header("Proporção Desejada")]
    public float targetAspectWidth = 16.0f;
    public float targetAspectHeight = 9.0f;

    void Update()
    {
        float targetAspect = targetAspectWidth / targetAspectHeight;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = GetComponent<Camera>();

        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}