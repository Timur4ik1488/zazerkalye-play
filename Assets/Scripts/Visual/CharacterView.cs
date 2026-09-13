using UnityEngine;

namespace Zazerkalye.Visual
{
    public class CameraBillboard : MonoBehaviour
    {
        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;
            transform.rotation = cam.transform.rotation;
        }
    }

    public static class CharacterView
    {
        public static GameObject Attach(string resourceName, Transform parent, float height)
        {
            var tex = SpriteCutout.Prepare(resourceName);
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.Destroy(go.GetComponent<Collider>());
            go.name = resourceName;
            go.transform.SetParent(parent, false);
            float aspect = tex != null ? tex.width / (float)tex.height : 0.75f;
            go.transform.localScale = new Vector3(height * aspect, height, 1f);
            go.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            var rend = go.GetComponent<Renderer>();
            rend.sharedMaterial = tex != null ? RuntimeMaterials.Textured(tex) : RuntimeMaterials.Lit(Color.magenta);
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            go.AddComponent<CameraBillboard>();
            return go;
        }
    }
}
