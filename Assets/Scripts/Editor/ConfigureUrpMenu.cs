using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Zazerkalye.EditorTools
{
    /// <summary>
    /// Ensures URP asset is assigned after package import (hand-authored YAML may need rebind).
    /// Menu: Зазеркалье → Configure URP
    /// </summary>
    public static class ConfigureUrpMenu
    {
        const string UrpPath = "Assets/Settings/UniversalRP.asset";

        [MenuItem("Зазеркалье/Configure URP")]
        public static void Configure()
        {
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpPath);
            if (urp == null)
            {
                Debug.LogError("URP asset not found at " + UrpPath + ". Create via Assets → Create → Rendering → URP Asset.");
                return;
            }

            GraphicsSettings.defaultRenderPipeline = urp;
            QualitySettings.renderPipeline = urp;
            EditorUtility.SetDirty(urp);
            AssetDatabase.SaveAssets();
            Debug.Log("URP assigned. Open Assets/Scenes/Bootstrap and press Play.");
        }

        [InitializeOnLoadMethod]
        static void AutoHint()
        {
            if (GraphicsSettings.defaultRenderPipeline == null)
                Debug.LogWarning("[Зазеркалье] URP not assigned. Run menu: Зазеркалье → Configure URP (after packages finish importing).");
        }
    }
}
