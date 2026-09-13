using System.Collections.Generic;
using UnityEngine;
using Zazerkalye.Data;
using Zazerkalye.Visual;

namespace Zazerkalye.World
{
    public class GroveBuilder : MonoBehaviour
    {
        public readonly List<Vector3> SwampCenters = new();
        public const float SwampRadius = 3.4f;

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

            var floor = new GameObject("GroundCollider");
            floor.transform.SetParent(root, false);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            var box = floor.AddComponent<BoxCollider>();
            box.size = new Vector3(MatchConfig.WorldRadius * 3f, 1f, MatchConfig.WorldRadius * 3f);

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
            var cap = tree.gameObject.AddComponent<CapsuleCollider>();
            cap.center = new Vector3(0f, h * 0.4f, 0f);
            cap.height = h * 0.85f;
            cap.radius = 0.32f * scale;
            int canopies = Random.Range(2, 4);
            for (int c = 0; c < canopies; c++)
            {
                var canopy = MeshFactory.Sphere($"Canopy_{c}",
                    Vector3.one * Random.Range(1.4f, 2.2f) * scale,
                    Color.Lerp(VisualPalette.Canopy, VisualPalette.Canopy * 1.2f, Random.value), tree);
                canopy.transform.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), h * 0.75f + c * 0.45f * scale, Random.Range(-0.3f, 0.3f));
            }
        }

        public void BuildSwamps(Transform root)
        {
            SwampCenters.Clear();
            for (int i = 0; i < 5; i++)
            {
                var p = Random.onUnitSphere; p.y = 0f;
                p = p.normalized * Random.Range(10f, MatchConfig.WorldRadius - 8f);
                SwampCenters.Add(p);
                var swamp = MeshFactory.Cylinder($"Swamp_{i}", new Vector3(SwampRadius, 0.05f, SwampRadius), VisualPalette.Swamp, root);
                swamp.transform.position = p + Vector3.up * 0.04f;
                swamp.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Transparent(VisualPalette.Swamp);
            }
        }

        public bool IsInSwamp(Vector3 pos)
        {
            var flat = new Vector3(pos.x, 0f, pos.z);
            float r2 = SwampRadius * SwampRadius;
            for (int i = 0; i < SwampCenters.Count; i++)
            {
                var c = SwampCenters[i];
                c.y = 0f;
                if ((flat - c).sqrMagnitude <= r2) return true;
            }
            return false;
        }
    }
}
