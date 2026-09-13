using System.Collections.Generic;
using UnityEngine;

namespace Zazerkalye.Visual
{
    /// <summary>Builds a chunky 3D figurine from a cutout portrait so characters have volume when the camera orbits.</summary>
    public static class VoxelFigure
    {
        static readonly Dictionary<string, Mesh> Cache = new();
        const int MaxHeight = 46;
        const int Depth = 8;
        const byte AlphaCut = 40;

        public static Mesh GetMesh(string key, Texture2D tex)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null)
                return cached;
            var mesh = tex != null ? Build(tex) : FallbackBox();
            mesh.name = key + "_voxel";
            Cache[key] = mesh;
            return mesh;
        }

        static Mesh FallbackBox()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mesh = Object.Instantiate(go.GetComponent<MeshFilter>().sharedMesh);
            Object.Destroy(go);
            return mesh;
        }

        static Mesh Build(Texture2D tex)
        {
            var src = tex.GetPixels32();
            int tw = tex.width, th = tex.height;
            int gh = Mathf.Clamp(MaxHeight, 16, 56);
            int gw = Mathf.Clamp(Mathf.RoundToInt(gh * tw / (float)th), 10, 40);

            var mask = new bool[gw * gh];

            for (int y = 0; y < gh; y++)
            for (int x = 0; x < gw; x++)
            {
                float u = (x + 0.5f) / gw;
                float v = (y + 0.5f) / gh;
                int sx = Mathf.Clamp((int)(u * tw), 0, tw - 1);
                int sy = Mathf.Clamp((int)(v * th), 0, th - 1);
                var c = src[sy * tw + sx];
                mask[y * gw + x] = c.a >= AlphaCut;
            }

            var dist = DistanceField(mask, gw, gh);
            var solid = new bool[gw * gh * Depth];
            for (int y = 0; y < gh; y++)
            for (int x = 0; x < gw; x++)
            {
                int i = y * gw + x;
                if (!mask[i]) continue;
                int thick = Mathf.Clamp(2 + dist[i] / 2, 2, Depth);
                int z0 = (Depth - thick) / 2;
                for (int z = z0; z < z0 + thick; z++)
                    solid[(z * gh + y) * gw + x] = true;
            }

            float cell = 1f / gh;
            float cellZ = 0.46f / Depth;
            var verts = new List<Vector3>(4096);
            var uvs = new List<Vector2>(4096);
            var tris = new List<int>(8192);

            bool Occ(int x, int y, int z) =>
                (uint)x < (uint)gw && (uint)y < (uint)gh && (uint)z < (uint)Depth &&
                solid[(z * gh + y) * gw + x];

            Vector3 Pos(int x, int y, int z) => new(
                (x - gw * 0.5f) * cell,
                y * cell,
                (z - Depth * 0.5f) * cellZ);

            void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int vx, int vy, Vector3 inside)
            {
                var n = Vector3.Cross(b - a, d - a);
                if (Vector3.Dot(n, (a + c) * 0.5f - inside) < 0f)
                    (b, d) = (d, b);
                float u0 = vx / (float)gw, u1 = (vx + 1) / (float)gw;
                float v0 = vy / (float)gh, v1 = (vy + 1) / (float)gh;
                int i = verts.Count;
                verts.Add(a); uvs.Add(new Vector2(u0, v0));
                verts.Add(b); uvs.Add(new Vector2(u1, v0));
                verts.Add(c); uvs.Add(new Vector2(u1, v1));
                verts.Add(d); uvs.Add(new Vector2(u0, v1));
                tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
                tris.Add(i); tris.Add(i + 2); tris.Add(i + 3);
            }

            for (int z = 0; z < Depth; z++)
            for (int y = 0; y < gh; y++)
            for (int x = 0; x < gw; x++)
            {
                if (!Occ(x, y, z)) continue;
                var p = Pos(x, y, z);
                var dx = Vector3.right * cell;
                var dy = Vector3.up * cell;
                var dz = Vector3.forward * cellZ;
                var mid = p + (dx + dy + dz) * 0.5f;
                if (!Occ(x, y, z + 1)) Face(p + dz, p + dx + dz, p + dx + dy + dz, p + dy + dz, x, y, mid);
                if (!Occ(x, y, z - 1)) Face(p + dx, p, p + dy, p + dx + dy, x, y, mid);
                if (!Occ(x + 1, y, z)) Face(p + dx, p + dx + dz, p + dx + dy + dz, p + dx + dy, x, y, mid);
                if (!Occ(x - 1, y, z)) Face(p + dz, p, p + dy, p + dy + dz, x, y, mid);
                if (!Occ(x, y + 1, z)) Face(p + dy, p + dx + dy, p + dx + dy + dz, p + dy + dz, x, y, mid);
                if (!Occ(x, y - 1, z)) Face(p, p + dz, p + dx + dz, p + dx, x, y, mid);
            }

            var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static int[] DistanceField(bool[] mask, int w, int h)
        {
            int n = w * h;
            var dist = new int[n];
            var q = new int[n];
            int head = 0, tail = 0;
            for (int i = 0; i < n; i++)
            {
                if (!mask[i]) { dist[i] = 0; q[tail++] = i; }
                else dist[i] = -1;
            }
            while (head < tail)
            {
                int i = q[head++];
                int x = i % w, y = i / w;
                int nd = dist[i] + 1;
                Try(x + 1, y); Try(x - 1, y); Try(x, y + 1); Try(x, y - 1);

                void Try(int nx, int ny)
                {
                    if ((uint)nx >= (uint)w || (uint)ny >= (uint)h) return;
                    int j = ny * w + nx;
                    if (dist[j] >= 0) return;
                    dist[j] = nd;
                    q[tail++] = j;
                }
            }
            return dist;
        }
    }
}
