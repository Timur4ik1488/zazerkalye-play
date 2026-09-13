using UnityEngine;

namespace Zazerkalye.Visual
{
    public static class SpriteCutout
    {
        static readonly System.Collections.Generic.Dictionary<string, Texture2D> Cache = new();

        public static Texture2D Prepare(string resourceName)
        {
            if (Cache.TryGetValue(resourceName, out var cached) && cached != null)
                return cached;
            Texture2D src = null;
            bool owned = false;
#if UNITY_EDITOR
            var path = System.IO.Path.Combine(Application.dataPath, "Resources", "Textures", resourceName + ".png");
            if (System.IO.File.Exists(path))
            {
                var data = System.IO.File.ReadAllBytes(path);
                src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                src.LoadImage(data);
                src.name = resourceName;
                owned = true;
            }
#endif
            if (src == null)
                src = Resources.Load<Texture2D>("Textures/" + resourceName);
            var prepared = Prepare(src);
            if (owned && src != null && src != prepared)
                Object.Destroy(src);
            if (prepared != null) Cache[resourceName] = prepared;
            return prepared;
        }

        public static Texture2D Prepare(Texture2D src)
        {
            if (src == null) return null;
            Color32[] px;
            try { px = src.GetPixels32(); }
            catch { return src; }

            int w = src.width, h = src.height;
            KnockoutBackground(px, w, h, 48);
            var boxed = CropToAlpha(px, w, h, out int nw, out int nh);
            var tex = new Texture2D(nw, nh, TextureFormat.RGBA32, false);
            tex.name = src.name + "_cut";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixels32(boxed);
            tex.Apply(false, false);
            return tex;
        }

        static void KnockoutBackground(Color32[] px, int w, int h, int thresh)
        {
            int[] samples =
            {
                0, w - 1, (h - 1) * w, h * w - 1,
                w / 2, (h - 1) * w + w / 2
            };
            int br = 0, bg = 0, bb = 0;
            var rs = new int[samples.Length];
            var gs = new int[samples.Length];
            var bs = new int[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                var c = px[samples[i]];
                rs[i] = c.r; gs[i] = c.g; bs[i] = c.b;
            }
            System.Array.Sort(rs); System.Array.Sort(gs); System.Array.Sort(bs);
            br = rs[samples.Length / 2];
            bg = gs[samples.Length / 2];
            bb = bs[samples.Length / 2];

            bool Near(Color32 c) =>
                Mathf.Abs(c.r - br) + Mathf.Abs(c.g - bg) + Mathf.Abs(c.b - bb) <= thresh;

            var seen = new bool[px.Length];
            var q = new int[px.Length];
            int head = 0, tail = 0;

            void Push(int i)
            {
                if (seen[i] || px[i].a == 0 || !Near(px[i])) return;
                seen[i] = true;
                q[tail++] = i;
            }

            for (int x = 0; x < w; x++)
            {
                Push(x);
                Push((h - 1) * w + x);
            }
            for (int y = 0; y < h; y++)
            {
                Push(y * w);
                Push(y * w + w - 1);
            }

            while (head < tail)
            {
                int i = q[head++];
                px[i] = new Color32(0, 0, 0, 0);
                int x = i % w, y = i / w;
                if (x > 0) Push(i - 1);
                if (x + 1 < w) Push(i + 1);
                if (y > 0) Push(i - w);
                if (y + 1 < h) Push(i + w);
            }
        }

        static Color32[] CropToAlpha(Color32[] px, int w, int h, out int nw, out int nh)
        {
            int minX = w, minY = h, maxX = 0, maxY = 0;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (px[y * w + x].a < 12) continue;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
            if (maxX <= minX || maxY <= minY)
            {
                nw = w; nh = h;
                return px;
            }
            const int pad = 6;
            minX = Mathf.Max(0, minX - pad);
            minY = Mathf.Max(0, minY - pad);
            maxX = Mathf.Min(w - 1, maxX + pad);
            maxY = Mathf.Min(h - 1, maxY + pad);
            nw = maxX - minX + 1;
            nh = maxY - minY + 1;
            var dst = new Color32[nw * nh];
            for (int y = 0; y < nh; y++)
                System.Array.Copy(px, (minY + y) * w + minX, dst, y * nw, nw);
            return dst;
        }
    }
}
