using UnityEngine;
namespace TimeGate.World
{
    /// Procedural textures/materials so the prototype needs no external art files.
    public static class Fx
    {
        static Texture2D glow, swirl;
        public static Texture2D Glow()
        {
            if (glow != null) return glow;
            const int n = 64; glow = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float dx = (x + .5f) / n * 2 - 1, dy = (y + .5f) / n * 2 - 1;
                float d = Mathf.Clamp01(1 - Mathf.Sqrt(dx * dx + dy * dy));
                float a = d * d;
                glow.SetPixel(x, y, new Color(1, 1, 1, a));
            }
            glow.wrapMode = TextureWrapMode.Clamp; glow.Apply(); return glow;
        }
        public static Texture2D Swirl()
        {
            if (swirl != null) return swirl;
            const int n = 256; swirl = new Texture2D(n, n, TextureFormat.RGBA32, true);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float dx = (x + .5f) / n * 2 - 1, dy = (y + .5f) / n * 2 - 1;
                float r = Mathf.Sqrt(dx * dx + dy * dy); if (r > 1) { swirl.SetPixel(x, y, Color.clear); continue; }
                float ang = Mathf.Atan2(dy, dx);
                float arms = 0.5f + 0.5f * Mathf.Sin(ang * 3f + r * 14f);
                float edge = Mathf.SmoothStep(1f, 0.82f, r);
                float core = Mathf.Pow(1f - r, 1.5f);
                float a = Mathf.Clamp01((arms * 0.7f + core * 0.8f) * edge);
                swirl.SetPixel(x, y, new Color(1, 1, 1, a));
            }
            swirl.wrapMode = TextureWrapMode.Clamp; swirl.Apply(); return swirl;
        }
        public static Material Additive(Color tint, Texture tex = null)
        {
            var m = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
            m.SetColor("_TintColor", tint); m.mainTexture = tex != null ? tex : Glow(); return m;
        }
        public static Material Lit(Color c, Texture tex = null, float smooth = 0.08f)
        {
            var m = new Material(Shader.Find("Standard")); m.color = c;
            if (tex != null) m.mainTexture = tex; m.SetFloat("_Glossiness", smooth); return m;
        }
        public static Texture2D Sandstone(Color a, Color b, int seed)
        {
            const int n = 256; var t = new Texture2D(n, n, TextureFormat.RGB24, true);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float strata = Mathf.PerlinNoise(x * 0.01f + seed, y * 0.09f + seed) * 0.6f;
                float grain = Mathf.PerlinNoise(x * 0.12f, y * 0.12f + seed) * 0.4f;
                t.SetPixel(x, y, Color.Lerp(a, b, Mathf.Clamp01(strata + grain)));
            }
            t.wrapMode = TextureWrapMode.Repeat; t.Apply(); return t;
        }
        public static GameObject Prim(PrimitiveType t, Transform parent, Vector3 pos, Vector3 scale, Material m, string name = null, bool collider = true)
        {
            var g = GameObject.CreatePrimitive(t); if (name != null) g.name = name;
            g.transform.SetParent(parent, false); g.transform.localPosition = pos; g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = m;
            if (!collider) Object.Destroy(g.GetComponent<Collider>());
            return g;
        }
    }
}
