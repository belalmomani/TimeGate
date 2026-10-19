using UnityEngine;
namespace TimeGate.World
{
    public class FlightCamera : MonoBehaviour
    {
        public Transform target; public SpiritVisual visual;
        public float sensitivity = 1f; public bool invertY; public bool firstPerson;
        public float yaw, pitch = 12f, distance = 7f;
        Vector3 smoothPos; bool init;
        public void Setup(Transform t, SpiritVisual v, float startYaw) { target = t; visual = v; yaw = startYaw; pitch = 12f; init = false; }
        public void ResetView() { if (target != null) yaw = target.eulerAngles.y; pitch = 12f; }
        public void SetFirstPerson(bool fp) { firstPerson = fp; if (visual != null) visual.SetVisible(!fp); }
        void LateUpdate()
        {
            if (target == null) return;
            var l = TouchInput.ConsumeLook() * (0.12f * sensitivity);
            yaw += l.x; pitch = Mathf.Clamp(pitch + l.y * (invertY ? -1f : 1f), -80f, 80f);
            var rot = Quaternion.Euler(pitch, yaw, 0);
            Vector3 pivot = target.position + Vector3.up * 0.5f;
            if (firstPerson) { transform.SetPositionAndRotation(pivot, rot); return; }
            Vector3 desired = pivot - rot * Vector3.forward * distance;
            Vector3 dir = desired - pivot; float len = dir.magnitude;
            if (Physics.SphereCast(pivot, 0.3f, dir / len, out var hit, len, ~0, QueryTriggerInteraction.Ignore)) desired = pivot + dir / len * Mathf.Max(0.6f, hit.distance - 0.1f);
            if (!init) { smoothPos = desired; init = true; }
            smoothPos = Vector3.Lerp(smoothPos, desired, 1f - Mathf.Exp(-14f * Time.deltaTime));
            transform.SetPositionAndRotation(smoothPos, rot);
        }
    }
}
