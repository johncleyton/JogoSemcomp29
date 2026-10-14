using UnityEngine;

// Câmera FIXA do minigame, no padrão do guia: ortográfica, size 5, posição (0, 0), 16:9.
// A quadra inteira cabe na tela, então não existe motivo pra câmera seguir as bolas
// (era isso que mostrava o fundo azul vazio).
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public float tamanho = 5f;
    // Cor de segurança caso algo apareça fora do fundo (em vez do azul padrão da Unity)
    public Color corDeFundo = new Color(0.10f, 0.12f, 0.10f);

    void Awake()
    {
        Camera cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = tamanho;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = corDeFundo;
        transform.position = new Vector3(0f, 0f, -10f);
    }
}