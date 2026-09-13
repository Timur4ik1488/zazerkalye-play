using UnityEngine;

namespace Zazerkalye.Visual
{
    public static class CharacterView
    {
        public static GameObject Attach(string resourceName, Transform parent, float height)
        {
            var tex = SpriteCutout.Prepare(resourceName);
            var go = new GameObject(resourceName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * height;

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = VoxelFigure.GetMesh(resourceName, tex);
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = tex != null
                ? RuntimeMaterials.Textured(tex)
                : RuntimeMaterials.Lit(Color.magenta);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mr.receiveShadows = true;
            return go;
        }
    }
}
