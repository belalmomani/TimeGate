using UnityEngine;
using TimeGate.Data;
namespace TimeGate.World
{
    /// Procedural Jerash (Gerasa) prototype: Hadrian's Arch, Oval Plaza, colonnaded Cardo, Temple of Artemis.
    /// ARTISTIC RECONSTRUCTION from primitives; proportions are approximate, not survey data.
    public static class JerashWorld
    {
        const float SX = 240f, SZ = 440f; const int N = 110;
        public static float Height(float x, float z)
        {
            float flat = Mathf.SmoothStep(0, 1, (Mathf.Abs(x) - 40f) / 60f);
            return 0.1f + Mathf.PerlinNoise(x * 0.03f + 4, z * 0.03f + 9) * 6f * flat + Mathf.PerlinNoise(x * 0.2f, z * 0.2f) * 0.2f;
        }

        public static WorldResult Build(SiteDef site)
        {
            var res = new WorldResult(); var root = new GameObject("World_Jerash"); res.root = root;
            RenderSettings.skybox = new Material(Shader.Find("Skybox/Procedural"));
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.6f, 0.7f, 0.85f); RenderSettings.ambientEquatorColor = new Color(0.6f, 0.55f, 0.45f); RenderSettings.ambientGroundColor = new Color(0.35f, 0.3f, 0.25f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Exponential; RenderSettings.fogDensity = 0.0025f; RenderSettings.fogColor = new Color(0.8f, 0.78f, 0.72f);
            var sun = new GameObject("Sun").AddComponent<Light>(); sun.transform.SetParent(root.transform);
            sun.type = LightType.Directional; sun.color = new Color(1f, 0.95f, 0.85f); sun.intensity = 1.2f; sun.shadows = LightShadows.Soft; sun.transform.rotation = Quaternion.Euler(50f, 30f, 0); RenderSettings.sun = sun;

            var lime = Fx.Lit(Color.white, Fx.Sandstone(new Color(0.78f, 0.72f, 0.58f), new Color(0.93f, 0.88f, 0.76f), 31), 0.05f);
            var paving = Fx.Lit(Color.white, Fx.Sandstone(new Color(0.45f, 0.42f, 0.38f), new Color(0.65f, 0.6f, 0.52f), 41), 0.05f);
            var ground = Fx.Lit(Color.white, Fx.Sandstone(new Color(0.55f, 0.5f, 0.32f), new Color(0.72f, 0.66f, 0.45f), 51), 0.02f);

            var tgo = new GameObject("Terrain"); tgo.transform.SetParent(root.transform);
            var mesh = new Mesh { name = "JerashTerrain" }; var vs = new Vector3[(N + 1) * (N + 1)]; var uvs = new Vector2[vs.Length]; var tri = new int[N * N * 6];
            for (int zi = 0; zi <= N; zi++) for (int xi = 0; xi <= N; xi++)
            { float x = (xi / (float)N - 0.5f) * SX, z = (zi / (float)N - 0.5f) * SZ; int k = zi * (N + 1) + xi; vs[k] = new Vector3(x, Height(x, z), z); uvs[k] = new Vector2(x / 14f, z / 14f); }
            int ti = 0;
            for (int zi = 0; zi < N; zi++) for (int xi = 0; xi < N; xi++)
            { int a = zi * (N + 1) + xi, b = a + 1, c = a + N + 1, d = c + 1; tri[ti++] = a; tri[ti++] = c; tri[ti++] = d; tri[ti++] = a; tri[ti++] = d; tri[ti++] = b; }
            mesh.vertices = vs; mesh.uv = uvs; mesh.triangles = tri; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            tgo.AddComponent<MeshFilter>().sharedMesh = mesh; tgo.AddComponent<MeshRenderer>().sharedMaterial = ground; tgo.AddComponent<MeshCollider>().sharedMesh = mesh;

            var T = root.transform;
            // Hadrian's Arch (z = -150): three passages approximated by pillars + lintels
            foreach (float px in new[] { -18f, -12f, -5.5f, 5.5f, 12f, 18f })
            { float h = Mathf.Abs(px) < 8 ? 16f : 11f; Fx.Prim(PrimitiveType.Cube, T, new Vector3(px, h / 2f, -150f), new Vector3(3.2f, h, 3.4f), lime, "ArchPillar"); }
            Fx.Prim(PrimitiveType.Cube, T, new Vector3(0, 17.2f, -150f), new Vector3(14.5f, 3f, 3.4f), lime, "ArchLintel");
            Fx.Prim(PrimitiveType.Cube, T, new Vector3(-15f, 11.8f, -150f), new Vector3(12f, 1.8f, 3.4f), lime, "ArchLintelL");
            Fx.Prim(PrimitiveType.Cube, T, new Vector3(15f, 11.8f, -150f), new Vector3(12f, 1.8f, 3.4f), lime, "ArchLintelR");

            // Oval Plaza (centre z = -85): paved ellipse ringed by columns
            const float ca = 22f, cb = 34f, cz = -85f;
            Fx.Prim(PrimitiveType.Cylinder, T, new Vector3(0, 0.15f, cz), new Vector3(ca * 2f, 0.1f, cb * 2f), paving, "OvalPaving", false);
            for (int i = 0; i < 40; i++) { float an = i / 40f * Mathf.PI * 2f; Column(T, lime, new Vector3(Mathf.Cos(an) * (ca + 1.5f), 0, cz + Mathf.Sin(an) * (cb + 1.5f)), 7f); }

            // Cardo: paved street z = -45 .. 150 with two rows of columns
            Fx.Prim(PrimitiveType.Cube, T, new Vector3(0, 0.15f, 52.5f), new Vector3(12f, 0.2f, 195f), paving, "Cardo", false);
            for (float z = -45f; z <= 150f; z += 8f) { Column(T, lime, new Vector3(-7f, 0, z), 8f); Column(T, lime, new Vector3(7f, 0, z), 8f); }

            // Temple of Artemis (z ~ 190): podium, 6 front columns, cella, pediment block
            Fx.Prim(PrimitiveType.Cube, T, new Vector3(0, 1.25f, 190f), new Vector3(30f, 2.5f, 36f), lime, "TemplePodium");
            for (int i = 0; i < 6; i++) Column(T, lime, new Vector3(-12f + i * 4.8f, 2.5f, 174f), 13f, 0.9f);
            Fx.Prim(PrimitiveType.Cube, T, new Vector3(0, 8.5f, 194f), new Vector3(16f, 12f, 24f), lime, "TempleCella");
            Fx.Prim(PrimitiveType.Cube, T, new Vector3(0, 16.2f, 175f), new Vector3(26f, 1.6f, 4f), lime, "TempleEntablature");

            var au = new GameObject("Wind").AddComponent<AudioSource>(); au.transform.SetParent(T); au.clip = PetraWorld.MakeWind(); au.loop = true; au.volume = 0.2f; au.spatialBlend = 0; au.Play();

            res.spawn = new Vector3(0, Height(0, -200f) + 7f, -200f); res.spawnYaw = 0f;
            res.bounds = new Bounds(new Vector3(0, 45, 0), new Vector3(SX - 4f, 100f, SZ - 4f));
            if (site.pois != null) foreach (var p in site.pois) res.pois.Add(new PoiRuntime { def = p, pos = new Vector3(p.x, p.y, p.z) });
            return res;
        }

        static void Column(Transform parent, Material m, Vector3 basePos, float height, float radius = 0.55f)
        {
            Fx.Prim(PrimitiveType.Cylinder, parent, basePos + Vector3.up * (height / 2f), new Vector3(radius * 2f, height / 2f, radius * 2f), m, "Column");
            Fx.Prim(PrimitiveType.Cube, parent, basePos + Vector3.up * (height + 0.3f), new Vector3(radius * 2.8f, 0.6f, radius * 2.8f), m, "Capital");
        }
    }
}
