#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Recorta a área transparente dos PNGs selecionados e configura o import como Sprite (pivot no centro).
/// Salva cópias "_recorte.png" ao lado; os originais NÃO são alterados.
///
/// - OVELHAS: cada arquivo é recortado sozinho e o pivot fica nos PÉS (centro-embaixo). Assim as 4 ovelhas
///   ficam alinhadas pelo chão e centradas, mesmo que tenham sido desenhadas em posições diferentes do canvas.
///   O PPU é o mesmo pra todas (a nova continua menor que a velha).
/// - TOSADOR: recorte comum entre os dois sprites, pivot no centro.
/// - BOTÕES: cada um sozinho, pivot no centro, 100 PPU.
///
/// Uso: Project > selecionar PNGs > botão direito > Tosquie > ...
/// </summary>
public static class TosquieSpriteTools
{
    [MenuItem("Assets/Tosquie/Recortar - OVELHAS (pivot nos pés, maior lado = 3 unidades)")]
    private static void Ovelhas() { Processar(3f, false, SpriteAlignment.BottomCenter); }

    [MenuItem("Assets/Tosquie/Recortar - TOSADOR (maior lado = 1.5 unidades)")]
    private static void Tosador() { Processar(1.5f, true, SpriteAlignment.Center); }

    // Botões: cada arquivo é recortado SOZINHO (pode selecionar os 6 de uma vez).
    // Antes eles dividiam o mesmo recorte e saíam largos, com a bolinha num canto.
    [MenuItem("Assets/Tosquie/Recortar - BOTÕES de UI (cada um sozinho, 100 PPU)")]
    private static void Botoes() { Processar(0f, false, SpriteAlignment.Center); }

    private class Item
    {
        public string path;
        public Texture2D tex;
        public int minX, minY, maxX, maxY;
    }

    // alvoUnidades <= 0: só recorta e mantém 100 PPU (bom pra Image de UI)
    private static void Processar(float alvoUnidades, bool mesmoRecorte, SpriteAlignment alinhamento)
    {
        List<Item> itens = new List<Item>();

        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;

            string minusculo = path.ToLowerInvariant();
            if (!minusculo.EndsWith(".png") || minusculo.EndsWith("_recorte.png")) continue;

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(File.ReadAllBytes(path)))
            {
                Debug.LogError("[Tosquie] Não consegui ler " + path);
                Object.DestroyImmediate(tex);
                continue;
            }

            Item item = new Item { path = path, tex = tex };
            if (!CalcularBordas(item))
            {
                Debug.LogWarning("[Tosquie] " + path + " está totalmente transparente.");
                Object.DestroyImmediate(tex);
                continue;
            }
            itens.Add(item);
        }

        if (itens.Count == 0)
        {
            Debug.LogWarning("[Tosquie] Selecione um ou mais arquivos .png no Project antes de usar este menu.");
            return;
        }

        // Canvases do mesmo tamanho -> mesmo recorte pra todos (une as áreas ocupadas)
        bool mesmoCanvas = true;
        foreach (Item it in itens)
            if (it.tex.width != itens[0].tex.width || it.tex.height != itens[0].tex.height) mesmoCanvas = false;

        if (mesmoRecorte && mesmoCanvas && itens.Count > 1)
        {
            int uMinX = int.MaxValue, uMinY = int.MaxValue, uMaxX = -1, uMaxY = -1;
            foreach (Item it in itens)
            {
                uMinX = Mathf.Min(uMinX, it.minX); uMinY = Mathf.Min(uMinY, it.minY);
                uMaxX = Mathf.Max(uMaxX, it.maxX); uMaxY = Mathf.Max(uMaxY, it.maxY);
            }
            foreach (Item it in itens)
            {
                it.minX = uMinX; it.minY = uMinY; it.maxX = uMaxX; it.maxY = uMaxY;
            }
        }
        else if (mesmoRecorte && itens.Count > 1)
        {
            Debug.LogWarning("[Tosquie] Os PNGs têm tamanhos de canvas diferentes: cada um foi recortado " +
                             "separadamente (a ovelha pode mudar de posição ao trocar de sprite).");
        }

        // PPU único pro grupo, baseado no maior recorte
        int maiorLado = 1;
        foreach (Item it in itens)
            maiorLado = Mathf.Max(maiorLado, Mathf.Max(it.maxX - it.minX + 1, it.maxY - it.minY + 1));
        float ppu = alvoUnidades > 0f ? maiorLado / alvoUnidades : 100f;

        foreach (Item it in itens)
            Salvar(it, ppu, alinhamento);

        foreach (Item it in itens)
            Object.DestroyImmediate(it.tex);
    }

    private static bool CalcularBordas(Item it)
    {
        int w = it.tex.width, h = it.tex.height;
        Color32[] px = it.tex.GetPixels32();
        it.minX = w; it.minY = h; it.maxX = -1; it.maxY = -1;

        for (int y = 0; y < h; y++)
        {
            int linha = y * w;
            for (int x = 0; x < w; x++)
            {
                if (px[linha + x].a <= 8) continue;
                if (x < it.minX) it.minX = x;
                if (x > it.maxX) it.maxX = x;
                if (y < it.minY) it.minY = y;
                if (y > it.maxY) it.maxY = y;
            }
        }
        return it.maxX >= 0;
    }

    private static void Salvar(Item it, float ppu, SpriteAlignment alinhamento)
    {
        int w = it.tex.width, h = it.tex.height;

        const int folga = 2;
        int x0 = Mathf.Max(0, it.minX - folga), y0 = Mathf.Max(0, it.minY - folga);
        int x1 = Mathf.Min(w - 1, it.maxX + folga), y1 = Mathf.Min(h - 1, it.maxY + folga);
        int cw = x1 - x0 + 1, ch = y1 - y0 + 1;

        Texture2D recorte = new Texture2D(cw, ch, TextureFormat.RGBA32, false);
        recorte.SetPixels(it.tex.GetPixels(x0, y0, cw, ch));
        recorte.Apply();

        string novoPath = Path.Combine(Path.GetDirectoryName(it.path),
                                       Path.GetFileNameWithoutExtension(it.path) + "_recorte.png").Replace('\\', '/');
        File.WriteAllBytes(novoPath, recorte.EncodeToPNG());
        Object.DestroyImmediate(recorte);

        AssetDatabase.ImportAsset(novoPath, ImportAssetOptions.ForceUpdate);

        TextureImporter ti = AssetImporter.GetAtPath(novoPath) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Bilinear;

            TextureImporterSettings s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteAlignment = (int)alinhamento;
            ti.SetTextureSettings(s);
            ti.SaveAndReimport();
        }

        Debug.Log("[Tosquie] " + it.path + " (" + w + "x" + h + ") -> " + novoPath + " (" + cw + "x" + ch +
                  ", PPU " + ppu.ToString("F1") + ")");
    }
}
#endif