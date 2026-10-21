using UnityEngine;
using TimeGate.Data;
namespace TimeGate.World
{
    /// Simple idle NPC that gently turns toward the camera when it is near.
    public class NpcIdle : MonoBehaviour
    {
        Vector3 basePos; float phase;
        void Start() { basePos = transform.position; phase = Random.value * 6f; }
        void Update()
        {
            transform.position = basePos + Vector3.up * Mathf.Sin(Time.time * 1.2f + phase) * 0.03f;
            var cam = Camera.main; if (cam == null) return;
            var to = cam.transform.position - transform.position; to.y = 0;
            if (to.magnitude < 18f && to.sqrMagnitude > 0.01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 2f * Time.deltaTime);
        }
    }
    public static class NpcBuilder
    {
        /// PLACEHOLDER figure from primitives (no licensed character model/skeleton yet).
        public static Transform Build(CharacterDef c, Vector3 groundPos, Transform parent)
        {
            var go = new GameObject("NPC_" + c.id); go.transform.SetParent(parent); go.transform.position = groundPos + Vector3.up * 0.95f;
            var cloth = Fx.Lit(new Color(0.55f, 0.32f, 0.2f)); var skin = Fx.Lit(new Color(0.72f, 0.52f, 0.38f)); var wrap = Fx.Lit(new Color(0.85f, 0.8f, 0.68f));
            Fx.Prim(PrimitiveType.Capsule, go.transform, Vector3.zero, new Vector3(0.75f, 0.95f, 0.75f), cloth, "Body");
            Fx.Prim(PrimitiveType.Sphere, go.transform, new Vector3(0, 1.12f, 0), Vector3.one * 0.38f, skin, "Head", false);
            Fx.Prim(PrimitiveType.Sphere, go.transform, new Vector3(0, 1.22f, 0), new Vector3(0.46f, 0.3f, 0.46f), wrap, "Headwrap", false);
            var marker = Fx.Prim(PrimitiveType.Quad, go.transform, new Vector3(0, 2.2f, 0), Vector3.one * 1.2f, Fx.Additive(new Color(0.99f, 0.8f, 0.3f, 0.7f)), "Marker", false);
            marker.AddComponent<Billboard>(); go.AddComponent<NpcIdle>(); return go.transform;
        }
    }
    public class Billboard : MonoBehaviour { void LateUpdate() { if (Camera.main != null) transform.rotation = Camera.main.transform.rotation; } }
}
