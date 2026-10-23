using System.Collections.Generic;
using UnityEngine;
using TimeGate.Data;
namespace TimeGate.World
{
    /// Procedural Petra prototype (Siq corridor + Treasury plaza). ARTISTIC RECONSTRUCTION from primitives.
    /// Replace with licensed scanned/modelled assets as they are acquired (see docs/ASSETS.md).
    public static class PetraWorld
    {
        const float SizeX = 200f, SizeZ = 400f; const int N = 160;

        static float Plaza(float z) => Mathf.SmoothStep(0, 1, (z - 95f) / 30f);
        static float CenterX(float z) => Mathf.Sin(z * 0.025f) * 18f * (1f - Plaza(z));
        public static float Height(float x, float z)
        {
            float plaza = Plaza(z);
            float d = Mathf.Abs(x - CenterX(z));
            float hw = Mathf.Lerp(5.5f, 48f, plaza);
            float wall = Mathf.SmoothStep(0, 1, (d - hw) / 14f);
            float n = Mathf.PerlinNoise(x * 0.045f + 10, z * 0.045f + 3) * 10f + Mathf.PerlinNoise(x * 0.2f, z * 0.2f) * 1.5f;
            float cliff = wall * (55f + n * 2f);
            float far = Mathf.SmoothStep(0, 1, (z - 150f) / 16f) * 75f;
            float nearWall = Mathf.SmoothStep(0, 1, (-182f - z) / 14f) * 60f;
            float floor = Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * 0.8f;
            return floor + cliff + far + nearWall;
        }

        public static WorldResult Build(SiteDef site)
        {
            var res = new WorldResult(); var root = new GameObject("World_Petra"); res.root = root;

            // sky, light, fog
            var sky = new Material(Shader.Find("Skybox/Procedural")); RenderSettings.skybox = sky;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.65f, 0.8f); RenderSettings.ambientEquatorColor = new Color(0.55f, 0.45f, 0.38f); RenderSettings.ambientGroundColor = new Color(0.3f, 0.22f, 0.18f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Exponential; RenderSettings.fogDensity = 0.0035f; RenderSettings.fogColor = new Color(0.78f, 0.7f, 0.62f);
            var sun = new GameObject("Sun").AddComponent<Light>(); sun.transform.SetParent(root.transform);
            sun.type = LightType.Directional; sun.color = new Color(1f, 0.93f, 0.8f); sun.intensity = 1.25f; sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -25f, 0); RenderSettings.sun = sun;

            // terrain
            var sand = Fx.Sandstone(new Color(0.78f, 0.45f, 0.30f), new Color(0.93f, 0.68f, 0.5f), 7);
            var mat = Fx.Lit(Color.white, sand); mat.mainTextureScale = new Vector2(1, 1);
            var tgo = new GameObject("Terrain"); tgo.transform.SetParent(root.transform);
            var mesh = new Mesh { name = "PetraTerrain" };
            var v = new Vector3[(N + 1) * (N + 1)]; var uv = new Vector2[v.Length]; var tri = new int[N * N * 6];
            for (int zi = 0; zi <= N; zi++) for (int xi = 0; xi <= N; xi++)
            {
                float x = (xi / (float)N - 0.5f) * SizeX, z = (zi / (float)N - 0.5f) * SizeZ; int i = zi * (N + 1) + xi;
                v[i] = new Vector3(x, Height(x, z), z); uv[i] = new Vector2(x / 14f, z / 14f);
            }
            int t = 0;
            for (int zi = 0; zi < N; zi++) for (int xi = 0; xi < N; xi++)
            {
                int a = zi * (N + 1) + xi, b = a + 1, c = a + N + 1, d = c + 1;
                tri[t++] = a; tri[t++] = c; tri[t++] = d; tri[t++] = a; tri[t++] = d; tri[t++] = b;
            }
            mesh.vertices = v; mesh.uv = uv; mesh.triangles = tri; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            tgo.AddComponent<MeshFilter>().sharedMesh = mesh; tgo.AddComponent<MeshRenderer>().sharedMaterial = mat;
            tgo.AddComponent<MeshCollider>().sharedMesh = mesh;

            // rocks along the Siq walls (low count, deterministic)
            var rng = new System.Random(1985); var rockMat = Fx.Lit(Color.white, Fx.Sandstone(new Color(0.6f, 0.36f, 0.26f), new Color(0.82f, 0.56f, 0.4f), 3));
            for (int i = 0; i < 60; i++)
            {
                float z = Mathf.Lerp(-170f, 120f, (float)rng.NextDouble()); float side = rng.NextDouble() < .5 ? -1f : 1f;
                float x = CenterX(z) + side * (4.2f + (float)rng.NextDouble() * 1.2f);
                float s = 0.8f + (float)rng.NextDouble() * 1.8f;
                var r = Fx.Prim(PrimitiveType.Sphere, root.transform, new Vector3(x, Height(x, z) + s * 0.3f, z), new Vector3(s * 1.3f, s, s * 1.1f), rockMat, "Rock");
                r.transform.rotation = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);
            }

            BuildTreasury(root.transform, Height(0, 147f));

            // wind (procedural audio, no external file)
            var au = new GameObject("Wind").AddComponent<AudioSource>(); au.transform.SetParent(root.transform);
            au.clip = MakeWind(); au.loop = true; au.volume = 0.3f; au.spatialBlend = 0; au.Play();

            // spawn + bounds + POIs
            float sz = -168f; res.spawn = new Vector3(CenterX(sz), Height(CenterX(sz), sz) + 6f, sz); res.spawnYaw = 0f;
            res.bounds = new Bounds(new Vector3(0, 55, 0), new Vector3(SizeX - 4f, 120f, SizeZ - 4f));
            if (site.pois != null) foreach (var p in site.pois) res.pois.Add(new PoiRuntime { def = p, pos = new Vector3(p.x, p.y, p.z) });
            if (site.characters != null)
                foreach (var c in site.characters)
                {
                    var npcTf = NpcBuilder.Build(c, new Vector3(c.x, Height(c.x, c.z), c.z), root.transform);
                    res.npcs.Add(new NpcRuntime { def = c, t = npcTf });
                }
            return res;
        }

        static void BuildTreasury(Transform parent, float baseY)
        {
            var g = new GameObject("Treasury_AlKhazneh_Placeholder"); g.transform.SetParent(parent); g.transform.position = new Vector3(0, baseY, 148f);
            var tex = Fx.Sandstone(new Color(0.85f, 0.52f, 0.4f), new Color(0.95f, 0.72f, 0.58f), 21);
            var m = Fx.Lit(Color.white, tex, 0.05f); var dark = Fx.Lit(new Color(0.08f, 0.06f, 0.05f), null, 0f);
            var T = g.transform;
            Fx.Prim(PrimitiveType.Cube, T, new Vector3(0, 19.5f, 3.5f), new Vector3(25, 39, 7), m, "Facade");
            for (int i = -3; i <= 2; i++) // lower portico (6 columns)
            {
                float x = (i + 0.5f) * 3.6f;
                Fx.Prim(PrimitiveType.Cylinder, T, new Vector3(x, 6.5f, -0.6f), new Vector3(1.8f, 6.5f, 1.8f), m, "Column");
                Fx.Prim(PrimitiveType.Cube, T, new Vector3(x, 13.2f, -0.6f), new Vector3(2.5f, 0.9f, 2.5f), m, "Capital");
            }
            Fx.Prim(PrimitiveType.Cube, T, new Vector3(0, 15f, -0.2f), new Vector3(23, 2.6f, 3.4f), m, "Entablature");
            Fx.Prim(PrimitiveType.Cube, T, new Vector3(0, 3.6f, -0.05f), new Vector3(3.4f, 7.2f, 0.5f), dark, "Doorway", false);
            Fx.Prim(PrimitiveType.Cylinder, T, new Vector3(0, 24f, 0.6f), new Vector3(8f, 5.5f, 8f), m, "Tholos");
            var dome = Fx.Prim(PrimitiveType.Sphere, T, new Vector3(0, 29.8f, 0.6f), new Vector3(8.2f, 3.6f, 8.2f), m, "TholosRoof");
            Fx.Prim(PrimitiveType.Sphere, T, new Vector3(0, 32.5f, 0.6f), new Vector3(1.2f, 1.6f, 1.2f), m, "Urn");
            var l = Fx.Prim(PrimitiveType.Cube, T, new Vector3(-7.8f, 21.5f, 0.2f), new Vector3(10.5f, 1.4f, 3f), m, "HalfPedimentL"); l.transform.localRotation = Quaternion.Euler(0, 0, 13);
            var r = Fx.Prim(PrimitiveType.Cube, T, new Vector3(7.8f, 21.5f, 0.2f), new Vector3(10.5f, 1.4f, 3f), m, "HalfPedimentR"); r.transform.localRotation = Quaternion.Euler(0, 0, -13);
            foreach (float x in new[] { -9.5f, -6f, 6f, 9.5f })
                Fx.Prim(PrimitiveType.Cylinder, T, new Vector3(x, 19f, 0f), new Vector3(1.3f, 3.2f, 1.3f), m, "UpperColumn");
        }

        internal static AudioClip MakeWind()
        {
            const int rate = 22050, n = rate * 6; var d = new float[n]; var rnd = new System.Random(5); float lp = 0, lp2 = 0;
            for (int i = 0; i < n; i++)
            {
                float w = (float)(rnd.NextDouble() * 2 - 1); lp += 0.02f * (w - lp); lp2 += 0.01f * (lp - lp2);
                float mod = 0.55f + 0.45f * Mathf.Sin(i / (float)n * Mathf.PI * 2f * 2f);
                float fade = Mathf.Min(1f, Mathf.Min(i, n - i) / 6000f);
                d[i] = lp2 * 9f * mod * fade;
            }
            var c = AudioClip.Create("wind", n, 1, rate, false); c.SetData(d, 0); return c;
        }
    }
}
