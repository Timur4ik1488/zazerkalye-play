using UnityEngine;
using UnityEngine.Rendering;

namespace Zazerkalye.Visual
{
    public static class VisualPalette
    {
        public static readonly Color Fog = new(0.12f, 0.18f, 0.17f, 1f);
        public static readonly Color Ambient = new(0.22f, 0.28f, 0.30f, 1f);
        public static readonly Color KeyLight = new(1.00f, 0.86f, 0.68f, 1f);
        public static readonly Color FillLight = new(0.35f, 0.48f, 0.55f, 1f);
        public static readonly Color Ground = new(0.16f, 0.22f, 0.18f, 1f);
        public static readonly Color GroundAccent = new(0.20f, 0.26f, 0.20f, 1f);
        public static readonly Color Trunk = new(0.18f, 0.14f, 0.12f, 1f);
        public static readonly Color Canopy = new(0.14f, 0.24f, 0.18f, 1f);
        public static readonly Color Swamp = new(0.12f, 0.22f, 0.20f, 0.72f);
        public static readonly Color PlayerCoat = new(0.28f, 0.32f, 0.34f, 1f);
        public static readonly Color PlayerSkin = new(0.72f, 0.58f, 0.48f, 1f);
        public static readonly Color Bobyl = new(0.78f, 0.72f, 0.55f, 1f);
        public static readonly Color HardBobyl = new(0.22f, 0.18f, 0.20f, 1f);
        public static readonly Color Jvachnik = new(0.42f, 0.55f, 0.38f, 1f);
        public static readonly Color Kukichi = new(0.86f, 0.62f, 0.28f, 1f);
        public static readonly Color KukichiBite = new(0.72f, 0.78f, 0.52f, 1f);
        public static readonly Color Shard = new(0.45f, 0.82f, 0.55f, 1f);
        public static readonly Color Pacanoid = new(0.38f, 0.78f, 0.42f, 1f);
        public static readonly Color Comb = new(0.72f, 0.58f, 0.38f, 1f);
        public static readonly Color Bone = new(0.86f, 0.82f, 0.74f, 1f);
        public static readonly Color Idol = new(0.38f, 0.36f, 0.34f, 1f);
        public static readonly Color Mirror = new(0.55f, 0.72f, 0.78f, 1f);
        public static readonly Color Well = new(0.35f, 0.42f, 0.55f, 1f);
        public static readonly Color UiBg = new(0.06f, 0.08f, 0.09f, 0.92f);
        public static readonly Color UiAccent = new(0.78f, 0.58f, 0.32f, 1f);
        public static readonly Color UiText = new(0.90f, 0.88f, 0.84f, 1f);
        public static readonly Color NightTint = new(0.08f, 0.10f, 0.16f, 1f);
    }

    public static class RuntimeMaterials
    {
        static Material _lit;

        public static Material Lit(Color color, float smoothness = 0.18f, float metallic = 0.05f)
        {
            Ensure();
            var m = new Material(_lit);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            else if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            return m;
        }

        public static Material Textured(Texture2D tex)
        {
            Ensure();
            var m = new Material(_lit);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
            else if (m.HasProperty("_Color")) m.SetColor("_Color", Color.white);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
            m.mainTexture = tex;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.14f);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0.02f);
            if (m.HasProperty("_Cutoff")) m.SetFloat("_Cutoff", 0.2f);
            if (m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip", 1f);
            m.EnableKeyword("_ALPHATEST_ON");
            return m;
        }

        public static Material Transparent(Color color)
        {
            var m = Lit(color, 0.1f, 0f);
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            return m;
        }

        static void Ensure()
        {
            if (_lit != null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                         ?? Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogError("[Зазеркалье] No lit/unlit shader found. Run menu Зазеркалье → Configure URP.");
                shader = Shader.Find("Hidden/InternalErrorShader");
            }
            _lit = new Material(shader);
        }
    }

    public static class MeshFactory
    {
        public static GameObject Capsule(string name, Vector3 scale, Color color, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Setup(go, name, scale, color, parent, false);
            return go;
        }

        public static GameObject Sphere(string name, Vector3 scale, Color color, Transform parent = null, bool transparent = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Setup(go, name, scale, color, parent, transparent);
            return go;
        }

        public static GameObject Cylinder(string name, Vector3 scale, Color color, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Setup(go, name, scale, color, parent, false);
            return go;
        }

        public static GameObject Plane(string name, Vector3 scale, Color color, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Setup(go, name, scale, color, parent, false);
            return go;
        }

        static void Setup(GameObject go, string name, Vector3 scale, Color color, Transform parent, bool transparent)
        {
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = transparent
                ? RuntimeMaterials.Transparent(color)
                : RuntimeMaterials.Lit(color);
        }
    }
}
