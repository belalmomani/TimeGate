using UnityEngine;
namespace TimeGate.World
{
    /// Free-flight movement with sphere-cast collision + slide, soft bounds and ground guard.
    public class SpiritController : MonoBehaviour
    {
        public Bounds worldBounds = new Bounds(Vector3.zero, new Vector3(400, 200, 400));
        public float cruise = 10f, fast = 32f, slow = 3.5f, accel = 18f, radius = 0.6f;
        public bool frozen; public bool Clamped { get; private set; }
        public FlightCamera cam; public Vector3 safePoint;
        Vector3 vel;

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Vector3 wish = Vector3.zero;
            if (!frozen && cam != null)
            {
                var f = cam.transform.forward; var r = cam.transform.right;
                wish = f * TouchInput.Move.y + r * TouchInput.Move.x + Vector3.up * TouchInput.Vertical;
                if (wish.sqrMagnitude > 1f) wish.Normalize();
            }
            float spd = TouchInput.Boost ? fast : (TouchInput.Slow ? slow : cruise);
            Vector3 target = wish * spd;
            float a = (target.sqrMagnitude > vel.sqrMagnitude ? accel : accel * 1.6f) * dt;
            vel = Vector3.MoveTowards(vel, target, a);

            Vector3 delta = vel * dt;
            for (int i = 0; i < 3 && delta.sqrMagnitude > 1e-8f; i++)
            {
                float d = delta.magnitude; Vector3 dir = delta / d;
                if (Physics.SphereCast(transform.position, radius, dir, out var hit, d + 0.02f, ~0, QueryTriggerInteraction.Ignore))
                {
                    float allow = Mathf.Max(0f, hit.distance - 0.02f);
                    transform.position += dir * allow;
                    Vector3 rem = delta - dir * allow;
                    delta = Vector3.ProjectOnPlane(rem, hit.normal); vel = Vector3.ProjectOnPlane(vel, hit.normal);
                }
                else { transform.position += delta; break; }
            }

            // ground guard against tunnelling through the terrain mesh
            var p = transform.position;
            if (Physics.Raycast(p + Vector3.up * 400f, Vector3.down, out var g, 800f, ~0, QueryTriggerInteraction.Ignore))
                if (p.y < g.point.y + radius + 0.25f) p.y = g.point.y + radius + 0.25f;
            var c = worldBounds.ClosestPoint(p); Clamped = (c - p).sqrMagnitude > 0.0001f; p = c;
            if (float.IsNaN(p.x + p.y + p.z)) p = safePoint;
            transform.position = p;

            if (vel.sqrMagnitude > 0.5f)
            {
                var flat = new Vector3(vel.x, 0, vel.z);
                if (flat.sqrMagnitude > 0.1f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 4f * dt);
            }
        }
        public void ResetTo(Vector3 pos) { transform.position = pos; vel = Vector3.zero; }
    }
}
