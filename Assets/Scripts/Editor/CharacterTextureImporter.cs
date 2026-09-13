using UnityEditor;
using UnityEngine;

namespace Zazerkalye.EditorTools
{
    public class CharacterTextureImporter : AssetPostprocessor
    {
        bool IsCharacterTexture => assetPath.Replace('\\', '/').Contains("Assets/Resources/Textures/");

        void OnPreprocessTexture()
        {
            if (!IsCharacterTexture) return;
            var imp = (TextureImporter)assetImporter;
            imp.textureType = TextureImporterType.Default;
            imp.alphaSource = TextureImporterAlphaSource.FromInput;
            imp.alphaIsTransparency = true;
            imp.npotScale = TextureImporterNPOTScale.None;
            imp.mipmapEnabled = false;
            imp.filterMode = FilterMode.Bilinear;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.isReadable = true;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.maxTextureSize = 1024;
        }
    }
}
