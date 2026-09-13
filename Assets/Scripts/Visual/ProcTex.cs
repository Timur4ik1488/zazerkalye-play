using System.Collections.Generic;
using UnityEngine;

namespace Zazerkalye.Visual
{
    /// <summary>Small painted maps so primitives stop looking like plastic candy.</summary>
    public static class ProcTex
    {
        static readonly Dictionary<string, Texture2D> Cache = new();
        const int N = 64;

        public static Texture2D For(Color c)
        {
            string key = $"{(int)(c.r * 255)}_{(int)(c.g * 255)}_{(int)(c.b * 255)}_{(int)(c.a * 255)}";
            if (Cache.TryGetValue(key, out var hit) && hit != null) return hit;
            float h, s, v;
            Color.RGBToHSV(c, out h, out s, out v);
            Texture2D tex;
            if (s < 0.18f && v < 0.22f) tex = Paint(key, c, Leather);
            else if (s < 0.25f && v > 0.75f) tex = Paint(key, c, Bone);
            else if (h < 0.08f || h > 0.92f) tex = Paint(key, c, v > 0.45f ? Skin : Leather);
            else if (h > 0.08f && h < 0.16f && s > 0.35f && v > 0.45f) tex = Paint(key, c, Metal);
            else if (h > 0.25f && h < 0.45f && v < 0.35f) tex = Paint(key, c, Bark);
            else if (h > 0.25f && h < 0.45f) tex = Paint(key, c, Moss);
            else if (h > 0.55f && h < 0.75f) tex = Paint(key, c, Stone);
            else tex = Paint(key, c, Fabric);
            Cache[key] = tex;
            return tex;
        }

        public static Texture2D Ground()
        {
            const string key = "ground";
            if (Cache.TryGetValue(key, out var hit) && hit != null) return hit;
            var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[128 * 128];
            for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.07f, y * 0.07f);
                float n2 = Mathf.PerlinNoise(x * 0.21f + 8f, y * 0.21f);
                var c = Color.Lerp(new Color(0.14f, 0.20f, 0.14f), new Color(0.22f, 0.30f, 0.18f), n);
                c = Color.Lerp(c, new Color(0.10f, 0.14f, 0.10f), n2 * 0.35f);
                if (((x * 13 + y * 7) % 47) == 0) c *= 0.7f;
                px[y * 128 + x] = c;
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            Cache[key] = tex;
            return tex;
        }

        public static Texture2D Arrow()
        {
            const string key = "arrow";
            if (Cache.TryGetValue(key, out var hit) && hit != null) return hit;
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[32 * 32];
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                float u = (x - 16) / 16f;
                float v = (y - 6) / 22f;
                bool tri = v > 0f && v < 1f && Mathf.Abs(u) < (1f - v) * 0.55f;
                px[y * 32 + x] = tri ? new Color32(255, 220, 120, 255) : new Color32(0, 0, 0, 0);
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            Cache[key] = tex;
            return tex;
        }

        static Texture2D Paint(string key, Color baseC, System.Func<int, int, Color, Color> fn)
        {
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
                px[y * N + x] = fn(x, y, baseC);
            tex.SetPixels32(px);
            tex.Apply(false, false);
            return tex;
        }

        static Color Fabric(int x, int y, Color c)
        {
            float n = Mathf.PerlinNoise(x * 0.18f, y * 0.18f);
            float fold = Mathf.PerlinNoise(x * 0.05f + 3f, y * 0.09f);
            float weave = ((x + y) % 2 == 0) ? 0.04f : 0f;
            return c * (0.72f + n * 0.28f + fold * 0.12f + weave);
        }

        static Color Leather(int x, int y, Color c)
        {
            float n = Mathf.PerlinNoise(x * 0.22f, y * 0.22f);
            float spec = Mathf.Pow(Mathf.PerlinNoise(x * 0.4f, y * 0.4f), 3f) * 0.15f;
            return c * (0.7f + n * 0.3f) + Color.white * spec;
        }

        static Color Metal(int x, int y, Color c)
        {
            float n = Mathf.PerlinNoise(x * 0.3f, y * 0.08f);
            float scratch = ((x * 19 + y * 11) % 29 == 0) ? 0.2f : 0f;
            return Color.Lerp(c * 0.65f, c * 1.15f, n) + Color.white * scratch;
        }

        static Color Skin(int x, int y, Color c)
        {
            float n = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
            return Color.Lerp(c * 0.88f, c * 1.08f, n);
        }

        static Color Bark(int x, int y, Color c)
        {
            float grain = Mathf.PerlinNoise(x * 0.4f, y * 0.06f);
            float knot = Mathf.PerlinNoise(x * 0.15f + 4f, y * 0.15f);
            var dark = c * 0.55f;
            return Color.Lerp(dark, c * 1.1f, grain) * (0.85f + knot * 0.2f);
        }

        static Color Moss(int x, int y, Color c)
        {
            float n = Mathf.PerlinNoise(x * 0.16f, y * 0.16f);
            float tuft = Mathf.PerlinNoise(x * 0.35f + 2f, y * 0.35f);
            return Color.Lerp(c * 0.6f, c * 1.25f, n) + new Color(0.05f, 0.08f, 0.02f) * tuft;
        }

        static Color Stone(int x, int y, Color c)
        {
            float n = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
            float crack = Mathf.PerlinNoise(x * 0.5f, y * 0.04f);
            var col = Color.Lerp(c * 0.7f, c * 1.1f, n);
            if (crack > 0.72f) col *= 0.55f;
            return col;
        }

        static Color Bone(int x, int y, Color c)
        {
            float n = Mathf.PerlinNoise(x * 0.14f, y * 0.14f);
            return Color.Lerp(c * 0.82f, c * 1.05f, n);
        }
    }
}
