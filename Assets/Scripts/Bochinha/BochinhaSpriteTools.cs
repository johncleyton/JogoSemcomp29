#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Cria uma cópia RECORTADA de cada PNG selecionado (só a bolinha, com pivot no centro dela)
/// e já configura o import como Sprite com o tamanho certo em unidades da Unity.
///
/// Como usar: selecione o(s) PNG(s) no Project > botão direito > Bochinha > Recortar bola...
/// O arquivo original NÃO é alterado; a cópia sai ao lado com o sufixo "_recorte".
/// Depois arraste o sprite "_recorte" pro SpriteRenderer do prefab correspondente.
/// </summary>
public static class BochinhaSpriteTools
{
    [MenuItem("Assets/Bochinha/Recortar bola - BOCHA (diâmetro 1.0)")]
    private static void RecortarBocha() { Processar(1.0f); }

    [MenuItem("Assets/Bochinha/Recortar bola - BOLIM (diâmetro 0.5)")]
    private static void RecortarBolim() { Processar(0.5f); }

    private static void Processar(float diametroAlvo)
    {
        int feitos = 0;

        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;

            string minusculo = path.ToLowerInvariant();
            if (!minusculo.EndsWith(".png") || minusculo.EndsWith("_recorte.png")) continue;

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(File.ReadAllBytes(path)))
            {
                Debug.LogError("[Bochinha] Não consegui ler " + path);
                Object.DestroyImmediate(tex);
                continue;
            }

            int w = tex.width, h = tex.height;
            Color32[] px = tex.GetPixels32();
            int minX = w, minY = h, maxX = -1, maxY = -1;

            for (int y = 0; y < h; y++)
            {
                int linha = y * w;
                for (int x = 0; x < w; x++)
                {
                    if (px[linha + x].a <= 8) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < 0)
            {
                Debug.LogWarning("[Bochinha] " + path + " está totalmente transparente.");
                Object.DestroyImmediate(tex);
                continue;
            }

            int diametroPx = Mathf.Max(maxX - minX + 1, maxY - minY + 1);

            // 2px de folga em volta (clamp nas bordas da imagem)
            const int folga = 2;
            int x0 = Mathf.Max(0, minX - folga), y0 = Mathf.Max(0, minY - folga);
            int x1 = Mathf.Min(w - 1, maxX + folga), y1 = Mathf.Min(h - 1, maxY + folga);
            int cw = x1 - x0 + 1, ch = y1 - y0 + 1;

            Texture2D recorte = new Texture2D(cw, ch, TextureFormat.RGBA32, false);
            recorte.SetPixels(tex.GetPixels(x0, y0, cw, ch));
            recorte.Apply();

            string novoPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_recorte.png");
            novoPath = novoPath.Replace('\\', '/');
            File.WriteAllBytes(novoPath, recorte.EncodeToPNG());
            AssetDatabase.ImportAsset(novoPath, ImportAssetOptions.ForceUpdate);

            TextureImporter ti = AssetImporter.GetAtPath(novoPath) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.spritePixelsPerUnit = diametroPx / diametroAlvo; // a bolinha passa a medir 'diametroAlvo' unidades
                ti.mipmapEnabled = false;
                ti.alphaIsTransparency = true;
                ti.filterMode = FilterMode.Bilinear;

                TextureImporterSettings s = new TextureImporterSettings();
                ti.ReadTextureSettings(s);
                s.spriteAlignment = (int)SpriteAlignment.Center; // pivot no centro da bolinha = onde o collider fica
                ti.SetTextureSettings(s);
                ti.SaveAndReimport();
            }

            Debug.Log("[Bochinha] " + path + " (" + w + "x" + h + ") -> " + novoPath + " (" + cw + "x" + ch +
                      ", bolinha de " + diametroPx + "px = " + diametroAlvo + " unidades). Arraste esse sprite pro prefab.");

            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(recorte);
            feitos++;
        }

        if (feitos == 0)
            Debug.LogWarning("[Bochinha] Selecione um ou mais arquivos .png no Project antes de usar este menu.");
    }
}
#endif
