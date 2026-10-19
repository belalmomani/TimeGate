using System.Collections;
using UnityEngine;
using TimeGate.Core;
namespace TimeGate.World
{
    public class Spinner : MonoBehaviour { public Vector3 axis = Vector3.forward; public float speed = 20f; void Update() { transform.Rotate(axis, speed * Time.deltaTime, Space.Self); } }

    public class DiceController : MonoBehaviour
    {
        static readonly Vector3[] N = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down };
        static readonly int[] Pips = { 1, 6, 2, 5, 3, 4 };
        public bool Rolling { get; private set; }
        public static Mesh BuildMesh()
        {
            var verts = new Vector3[24]; var uvs = new Vector2[24]; var tris = new int[36];
            for (int f = 0; f < 6; f++)
            {
                Vector3 n = N[f]; Vector3 u = (f >= 4) ? Vector3.forward : Vector3.up; Vector3 r = Vector3.Cross(n, u);
                Vector3 c = n * 0.5f; int b = f * 4;
                verts[b] = c - r * 0.5f - u * 0.5f; verts[b + 1] = c - r * 0.5f + u * 0.5f; verts[b + 2] = c + r * 0.5f + u * 0.5f; verts[b + 3] = c + r * 0.5f - u * 0.5f;
                int col = f % 3, row = f / 3; float u0 = col / 3f, u1 = (col + 1) / 3f, v0 = row / 2f, v1 = (row + 1) / 2f;
                uvs[b] = new Vector2(u0, v0); uvs[b + 1] = new Vector2(u0, v1); uvs[b + 2] = new Vector2(u1, v1); uvs[b + 3] = new Vector2(u1, v0);
                int t = f * 6; tris[t] = b; tris[t + 1] = b + 1; tris[t + 2] = b + 2; tris[t + 3] = b; tris[t + 4] = b + 2; tris[t + 5] = b + 3;
            }
            var m = new Mesh { name = "Dice" }; m.vertices = verts; m.uv = uvs; m.triangles = tris; m.RecalculateNormals(); return m;
        }
        public static Texture2D BuildTexture()
        {
            const int cell = 128; var t = new Texture2D(cell * 3, cell * 2, TextureFormat.RGB24, false);
            var ivory = new Color(0.93f, 0.86f, 0.68f); var ink = new Color(0.05f, 0.12f, 0.2f);
            for (int y = 0; y < cell * 2; y++) for (int x = 0; x < cell * 3; x++) t.SetPixel(x, y, ivory);
            for (int f = 0; f < 6; f++)
            {
                int col = f % 3, row = f / 3, cx0 = col * cell, cy0 = row * cell;
                foreach (var p in PipPositions(Pips[f])) Disc(t, cx0 + (int)(p.x * cell), cy0 + (int)(p.y * cell), 13, ink);
            }
            t.Apply(); return t;
        }
        static Vector2[] PipPositions(int n)
        {
            float a = 0.27f, b = 0.5f, c = 0.73f; var C = new Vector2(b, b);
            switch (n)
            {
                case 1: return new[] { C };
                case 2: return new[] { new Vector2(a, a), new Vector2(c, c) };
                case 3: return new[] { new Vector2(a, a), C, new Vector2(c, c) };
                case 4: return new[] { new Vector2(a, a), new Vector2(c, a), new Vector2(a, c), new Vector2(c, c) };
                case 5: return new[] { new Vector2(a, a), new Vector2(c, a), C, new Vector2(a, c), new Vector2(c, c) };
                default: return new[] { new Vector2(a, a), new Vector2(c, a), new Vector2(a, b), new Vector2(c, b), new Vector2(a, c), new Vector2(c, c) };
            }
        }
        static void Disc(Texture2D t, int cx, int cy, int r, Color col)
        { for (int y = -r; y <= r; y++) for (int x = -r; x <= r; x++) if (x * x + y * y <= r * r) t.SetPixel(cx + x, cy + y, col); }

        public IEnumerator Roll(int faceIndex)
        {
            Rolling = true; var axis = Random.onUnitSphere; float speed = 720f; float t = 0, dur = 2.2f;
            Vector3 basePos = transform.position;
            while (t < dur)
            {
                t += Time.deltaTime; float k = 1f - t / dur; transform.Rotate(axis, speed * k * Time.deltaTime, Space.World);
                transform.position = basePos + Vector3.up * Mathf.Abs(Mathf.Sin(t * 6f)) * 0.8f * k; yield return null;
            }
            var start = transform.rotation; var end = Quaternion.FromToRotation(N[faceIndex], Vector3.back);
            for (float s = 0; s < 1f; s += Time.deltaTime * 3f) { transform.rotation = Quaternion.Slerp(start, end, s); transform.position = basePos; yield return null; }
            transform.rotation = end; transform.position = basePos; Rolling = false;
        }
    }

    public static class GateRoom
    {
        public static GameObject Build(out DiceController dice)
        {
            var root = new GameObject("GateRoom");
            RenderSettings.skybox = null; RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Exponential; RenderSettings.fogDensity = 0.02f; RenderSettings.fogColor = Palette.Night;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.12f, 0.16f, 0.24f);
            var stone = Fx.Lit(Color.white, Fx.Sandstone(new Color(0.16f, 0.17f, 0.2f), new Color(0.32f, 0.33f, 0.38f), 11), 0.1f);
            var gold = Fx.Lit(Palette.Gold, null, 0.6f);
            // floor
            Fx.Prim(PrimitiveType.Cube, root.transform, new Vector3(0, -0.5f, 0), new Vector3(60, 1, 60), stone, "Floor");
            // ring of blocks
            var ring = new GameObject("Ring"); ring.transform.SetParent(root.transform, false); ring.transform.localPosition = new Vector3(0, 5.2f, 3f);
            const int count = 30; const float R = 4.6f;
            for (int i = 0; i < count; i++)
            {
                float a = i / (float)count * Mathf.PI * 2f; var p = new Vector3(Mathf.Cos(a) * R, Mathf.Sin(a) * R, 0);
                var b = Fx.Prim(PrimitiveType.Cube, ring.transform, p, new Vector3(1.5f, 1.35f, 1.4f), (i % 5 == 0) ? gold : stone, "Block");
                b.transform.localRotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg + 90f);
            }
            // pillars
            Fx.Prim(PrimitiveType.Cube, root.transform, new Vector3(-6.4f, 2.6f, 3f), new Vector3(1.8f, 5.2f, 1.8f), stone, "PillarL");
            Fx.Prim(PrimitiveType.Cube, root.transform, new Vector3(6.4f, 2.6f, 3f), new Vector3(1.8f, 5.2f, 1.8f), stone, "PillarR");
            // portal disc
            var disc = GameObject.CreatePrimitive(PrimitiveType.Quad); Object.Destroy(disc.GetComponent<Collider>()); disc.name = "Portal";
            disc.transform.SetParent(ring.transform, false); disc.transform.localScale = Vector3.one * 8.6f; disc.transform.localPosition = new Vector3(0, 0, 0.1f);
            disc.GetComponent<Renderer>().sharedMaterial = Fx.Additive(new Color(0.35f, 0.85f, 1f, 0.9f), Fx.Swirl()); disc.AddComponent<Spinner>().speed = 25f;
            var disc2 = Object.Instantiate(disc, ring.transform); disc2.transform.localScale = Vector3.one * 6.4f; disc2.GetComponent<Spinner>().speed = -40f;
            var pl = new GameObject("PortalLight").AddComponent<Light>(); pl.transform.SetParent(root.transform, false); pl.transform.position = new Vector3(0, 5.2f, -1f);
            pl.type = LightType.Point; pl.color = Palette.Energy; pl.range = 22; pl.intensity = 3.2f;
            var dl = new GameObject("Dir").AddComponent<Light>(); dl.transform.SetParent(root.transform, false); dl.type = LightType.Directional; dl.color = new Color(0.6f, 0.7f, 1f); dl.intensity = 0.5f; dl.transform.rotation = Quaternion.Euler(35, -20, 0);
            // dice
            var dgo = new GameObject("Dice"); dgo.transform.SetParent(root.transform, false); dgo.transform.position = new Vector3(0, 1.5f, -2.2f); dgo.transform.localScale = Vector3.one * 1.8f;
            dgo.AddComponent<MeshFilter>().sharedMesh = DiceController.BuildMesh();
            dgo.AddComponent<MeshRenderer>().sharedMaterial = Fx.Lit(Color.white, DiceController.BuildTexture(), 0.4f);
            dice = dgo.AddComponent<DiceController>(); dgo.SetActive(false);
            return root;
        }
    }
}
