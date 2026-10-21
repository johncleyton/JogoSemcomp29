using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraAspectFixo : MonoBehaviour
{
    [Header("Proporção Desejada")]
    public float targetAspectWidth = 16.0f;
    public float targetAspectHeight = 9.0f;

    private Camera cameraPrincipal;
    private Camera cameraFundo;

    private void OnEnable()
    {
        cameraPrincipal = GetComponent<Camera>();
        AtualizarViewport();

        // A camera recortada nao limpa os pixels das faixas laterais/superiores.
        // No Android esses pixels podem conter dados indefinidos a cada quadro.
        // Uma camera vazia limpa a tela inteira antes das cameras do jogo.
        if (Application.IsPlaying(gameObject))
        {
            GameObject fundo = new GameObject("FundoPreto16x9", typeof(Camera));
            fundo.transform.SetParent(transform, false);
            cameraFundo = fundo.GetComponent<Camera>();
            cameraFundo.clearFlags = CameraClearFlags.SolidColor;
            cameraFundo.backgroundColor = Color.black;
            cameraFundo.cullingMask = 0;
            cameraFundo.rect = new Rect(0f, 0f, 1f, 1f);
            cameraFundo.allowHDR = false;
            cameraFundo.allowMSAA = false;
            cameraFundo.useOcclusionCulling = false;

            var dados = fundo.AddComponent<UniversalAdditionalCameraData>();
            dados.renderType = CameraRenderType.Base;
            dados.renderShadows = false;
            dados.renderPostProcessing = false;
            dados.requiresColorOption = CameraOverrideOption.Off;
            dados.requiresDepthOption = CameraOverrideOption.Off;
            dados.volumeLayerMask = 0;
            AtualizarFundo();
        }
    }

    private void LateUpdate()
    {
        AtualizarViewport();
        AtualizarFundo();
    }

    private void AtualizarViewport()
    {
        if (cameraPrincipal == null || Screen.width <= 0 || Screen.height <= 0
            || targetAspectWidth <= 0f || targetAspectHeight <= 0f)
            return;

        float targetAspect = targetAspectWidth / targetAspectHeight;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = cameraPrincipal;

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

    private void AtualizarFundo()
    {
        if (cameraFundo == null || cameraPrincipal == null)
            return;

        cameraFundo.depth = cameraPrincipal.depth - 100f;
        cameraFundo.targetDisplay = cameraPrincipal.targetDisplay;
        // Nao limpar a tela para cameras usadas apenas em RenderTextures.
        bool temFaixas = cameraPrincipal.rect.width < 1f || cameraPrincipal.rect.height < 1f;
        cameraFundo.enabled = temFaixas && cameraPrincipal.enabled
            && cameraPrincipal.targetTexture == null;
    }

    private void OnDisable()
    {
        if (cameraFundo == null)
            return;

        cameraFundo.enabled = false;
        if (Application.isPlaying)
            Destroy(cameraFundo.gameObject);
        else
            DestroyImmediate(cameraFundo.gameObject);
        cameraFundo = null;
    }
}
