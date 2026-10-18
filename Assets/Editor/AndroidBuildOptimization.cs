using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Ferramentas apenas do Editor: nao entram no APK.
public static class AndroidBuildOptimization
{
    private const string OutputDirectory = "Builds/Android";

    [MenuItem("Semcomp/Build/Otimizar texturas Android")]
    public static void OptimizeTextures()
    {
        RequireAndroid();
        var roots = EnabledScenes().Concat(AssetDatabase.GetAllAssetPaths()
            .Where(path => path.StartsWith("Assets/", StringComparison.Ordinal)
                && path.Contains("/Resources/") && !AssetDatabase.IsValidFolder(path)))
            .ToArray();
        var dependencies = new HashSet<string>(AssetDatabase.GetDependencies(roots, true));
        int changed = 0;
        int skipped = 0;

        foreach (string path in dependencies.OrderBy(path => path, StringComparer.Ordinal))
        {
            if (!path.StartsWith("Assets/Sprites/", StringComparison.Ordinal))
                continue;

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Sprite)
                continue;

            var settings = importer.GetPlatformTextureSettings("Android");
            // Preserva overrides manuais e texturas lidas pelo jogo (pintura/cursor).
            if (importer.isReadable || settings.overridden)
            {
                skipped++;
                continue;
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            // ETC exige blocos de 4 pixels. Nao redimensionar sprites para satisfazer isso.
            if (texture == null || texture.width % 4 != 0 || texture.height % 4 != 0)
            {
                skipped++;
                continue;
            }

            // Parte do default efetivo, inclusive Max Size, sem mudar outras plataformas.
            settings = importer.GetDefaultPlatformTextureSettings();
            settings.name = "Android";
            settings.overridden = true;
            settings.format = TextureImporterFormat.ETC2_RGBA8Crunched;
            settings.textureCompression = TextureImporterCompression.CompressedHQ;
            settings.crunchedCompression = true;
            settings.compressionQuality = 100;
            settings.allowsAlphaSplitting = false;
            importer.SetPlatformTextureSettings(settings);
            importer.SaveAndReimport();
            changed++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Android: {changed} texturas otimizadas; {skipped} preservadas. Resolucao, recortes e pivots mantidos.");
    }

    [MenuItem("Semcomp/Build/APK release otimizado")]
    public static void BuildRelease()
    {
        Build("SemcompJogo.apk");
    }

    // Para validacao local/CI sem expor a senha da chave de publicacao.
    // Este APK usa a chave de debug e nao substitui uma versao assinada para distribuicao.
    public static void ValidateBuild()
    {
        bool customKeystore = PlayerSettings.Android.useCustomKeystore;
        try
        {
            PlayerSettings.Android.useCustomKeystore = false;
            OptimizeTextures();
            Build("SemcompJogo-validacao.apk");
        }
        finally
        {
            PlayerSettings.Android.useCustomKeystore = customKeystore;
            AssetDatabase.SaveAssets();
        }
    }

    private static void Build(string filename)
    {
        RequireAndroid();
        var scenes = EnabledScenes();
        if (scenes.Length == 0)
            throw new BuildFailedException("Nenhuma cena habilitada em Build Settings.");

        Directory.CreateDirectory(OutputDirectory);
        bool appBundle = EditorUserBuildSettings.buildAppBundle;
        bool exportProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;
        var subtarget = EditorUserBuildSettings.androidBuildSubtarget;
        try
        {
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.Generic;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                target = BuildTarget.Android,
                locationPathName = Path.Combine(OutputDirectory, filename),
                options = BuildOptions.CompressWithLz4HC | BuildOptions.DetailedBuildReport
            });
            SaveReport(report, filename);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Build Android: " + report.summary.result);

            long apkBytes = new FileInfo(report.summary.outputPath).Length;
            Debug.Log($"APK gerado: {report.summary.outputPath} ({apkBytes / 1048576.0:F2} MiB)");
        }
        finally
        {
            EditorUserBuildSettings.buildAppBundle = appBundle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = exportProject;
            EditorUserBuildSettings.androidBuildSubtarget = subtarget;
        }
    }

    private static void SaveReport(BuildReport report, string filename)
    {
        var data = new SizeReport
        {
            unityVersion = Application.unityVersion,
            result = report.summary.result.ToString(),
            // totalSize pode incluir outros artefatos; medir o APK que sera distribuido.
            apkBytes = report.summary.result == BuildResult.Succeeded && File.Exists(report.summary.outputPath)
                ? (ulong)new FileInfo(report.summary.outputPath).Length
                : 0,
            scenes = EnabledScenes(),
            assets = report.packedAssets.SelectMany(pack => pack.contents)
                .GroupBy(asset => asset.sourceAssetPath)
                .Select(group => new AssetSize
                {
                    path = group.Key,
                    packedBytes = group.Aggregate(0UL, (size, asset) => size + asset.packedSize)
                })
                .OrderByDescending(asset => asset.packedBytes).ToArray()
        };
        File.WriteAllText(Path.Combine(OutputDirectory, filename + ".report.json"),
            JsonUtility.ToJson(data, true));
    }

    private static string[] EnabledScenes()
    {
        return EditorBuildSettings.scenes.Where(scene => scene.enabled)
            .Select(scene => scene.path).ToArray();
    }

    private static void RequireAndroid()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            throw new BuildFailedException("Selecione Android e execute Switch Platform antes de usar esta ferramenta.");
    }

    [Serializable]
    private class SizeReport
    {
        public string unityVersion;
        public string result;
        public ulong apkBytes;
        public string[] scenes;
        public AssetSize[] assets;
    }

    [Serializable]
    private class AssetSize
    {
        public string path;
        public ulong packedBytes;
    }
}
