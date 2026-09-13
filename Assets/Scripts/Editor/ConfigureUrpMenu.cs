using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Zazerkalye.EditorTools
{
    /// <summary>
    /// Creates and assigns a URP asset. Hand-authored YAML often fails to import in Unity 6.3.
    /// Menu: Зазеркалье → Configure URP
    /// </summary>
    public static class ConfigureUrpMenu
    {
        const string SettingsFolder = "Assets/Settings";
        const string UrpPath = "Assets/Settings/UniversalRP.asset";
        const string RendererPath = "Assets/Settings/UniversalRenderer.asset";

        [MenuItem("Зазеркалье/Configure URP")]
        public static void Configure()
        {
            var urp = EnsureUrp();
            if (urp == null)
            {
                EditorUtility.DisplayDialog(
                    "Зазеркалье",
                    "Не удалось создать URP. Дождитесь конца импорта пакетов (правый нижний угол) и повторите Зазеркалье → Configure URP.",
                    "OK");
                return;
            }

            Assign(urp);
            EditorUtility.DisplayDialog(
                "Зазеркалье",
                "URP назначен. Откройте Assets/Scenes/Bootstrap и нажмите Play.",
                "OK");
            Debug.Log("[Зазеркалье] URP assigned. Open Assets/Scenes/Bootstrap and press Play.");
        }

        [InitializeOnLoadMethod]
        static void AutoConfigure()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset) return;
                var urp = EnsureUrp();
                if (urp != null) Assign(urp);
            };
        }

        static UniversalRenderPipelineAsset EnsureUrp()
        {
            if (!AssetDatabase.IsValidFolder(SettingsFolder))
                AssetDatabase.CreateFolder("Assets", "Settings");

            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                ReplaceBroken(RendererPath);
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }

            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpPath);
            if (urp == null)
            {
                ReplaceBroken(UrpPath);
                urp = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(urp, UrpPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpPath);
        }

        static void ReplaceBroken(string path)
        {
            var existing = AssetDatabase.LoadMainAssetAtPath(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);
            else if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
                AssetDatabase.DeleteAsset(path);
        }

        static void Assign(UniversalRenderPipelineAsset urp)
        {
            GraphicsSettings.defaultRenderPipeline = urp;
            QualitySettings.renderPipeline = urp;
            var current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = urp;
            }
            QualitySettings.SetQualityLevel(current, true);
            EditorUtility.SetDirty(urp);
            AssetDatabase.SaveAssets();
        }
    }
}
