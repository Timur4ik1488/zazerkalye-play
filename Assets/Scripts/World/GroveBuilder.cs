using UnityEngine;
using Zazerkalye.Data;
using Zazerkalye.Visual;

namespace Zazerkalye.World
{
    public class GroveBuilder : MonoBehaviour
    {
        public void Build(Transform root)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.028f;
            RenderSettings.fogColor = VisualPalette.Fog;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = VisualPalette.Ambient;

            var ground = MeshFactory.Plane("Ground", new Vector3(10f, 1f, 10f), VisualPalette.Ground, root);
            ground.transform.position = Vector3.zero;

            for (int i = 0; i < 18; i++)
            {
                var p = Random.onUnitSphere; p.y = 0f;
                p = p.normalized * Random.Range(4f, MatchConfig.WorldRadius - 3f);
                var patch = MeshFactory.Plane($"Moss_{i}", new Vector3(0.6f, 1f, 0.6f), VisualPalette.GroundAccent, root);
                patch.transform.position = p + Vector3.up * 0.02f;
                patch.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }

            for (int i = 0; i < 48; i++)
            {
                var p = Random.onUnitSphere; p.y = 0f;
                p = p.normalized * Random.Range(6f, MatchConfig.WorldRadius - 2f);
                BuildTree(root, p, i);
            }

            for (int i = 0; i < 24; i++)
            {
                float ang = i / 24f * Mathf.PI * 2f;
                var p = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * (MatchConfig.WorldRadius + 1.5f);
                BuildTree(root, p, 100 + i, 1.3f);
            }
        }

        void BuildTree(Transform root, Vector3 pos, int i, float scale = 1f)
        {
            var tree = new GameObject($"Tree_{i}").transform;
            tree.SetParent(root, false);
            tree.position = pos;
            float h = Random.Range(2.4f, 4.2f) * scale;
            var trunk = MeshFactory.Cylinder("Trunk", new Vector3(0.28f * scale, h * 0.5f, 0.28f * scale), VisualPalette.Trunk, tree);
            trunk.transform.localPosition = new Vector3(0f, h * 0.5f, 0f);
            int canopies = Random.Range(2, 4);
            for (int c = 0; c < canopies; c++)
            {
                var canopy = MeshFactory.Sphere($"Canopy_{c}",
                    Vector3.one * Random.Range(1.4f, 2.2f) * scale,
                    Color.Lerp(VisualPalette.Canopy, VisualPalette.Canopy * 1.2f, Random.value), tree);
                canopy.transform.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), h * 0.75f + c * 0.45f * scale, Random.Range(-0.3f, 0.3f));
            }
        }

        public void BuildSwamps(Transform root, System.Action<Collider> onEnter, System.Action<Collider> onExit)
        {
            for (int i = 0; i < 5; i++)
            {
                var p = Random.onUnitSphere; p.y = 0f;
                p = p.normalized * Random.Range(10f, MatchConfig.WorldRadius - 8f);
                var swamp = MeshFactory.Cylinder($"Swamp_{i}", new Vector3(3.2f, 0.05f, 3.2f), VisualPalette.Swamp, root);
                swamp.transform.position = p + Vector3.up * 0.04f;
                swamp.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Transparent(VisualPalette.Swamp);
                var col = swamp.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 0.55f;
                var zone = swamp.AddComponent<SwampZone>();
                zone.OnPlayerEnter = onEnter;
                zone.OnPlayerExit = onExit;
            }
        }
    }

    public class SwampZone : MonoBehaviour
    {
        public System.Action<Collider> OnPlayerEnter;
        public System.Action<Collider> OnPlayerExit;

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<Zazerkalye.Player.PlayerController>() != null)
                OnPlayerEnter?.Invoke(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<Zazerkalye.Player.PlayerController>() != null)
                OnPlayerExit?.Invoke(other);
        }
    }
}
