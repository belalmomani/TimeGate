using UnityEngine;
namespace TimeGate.World
{
    /// Procedural energy spirit: ellipsoid aura + bright core + billboard halo + particles + light trail.
    public class SpiritVisual : MonoBehaviour
    {
        Renderer[] renders; ParticleSystem ps; TrailRenderer trail; Transform halo; Light lamp; bool reduce;
        public void Init(bool reduceEffects)
        {
            reduce = reduceEffects;
            var energy = Core.Palette.Energy;
            var aura = Fx.Prim(PrimitiveType.Sphere, transform, Vector3.zero, new Vector3(0.9f, 1.5f, 0.9f), Fx.Additive(new Color(energy.r, energy.g, energy.b, 0.28f)), "Aura", false);
            var core = Fx.Prim(PrimitiveType.Sphere, transform, new Vector3(0, 0.1f, 0), Vector3.one * 0.38f, Fx.Additive(new Color(0.9f, 1f, 1f, 0.9f)), "Core", false);
            var head = Fx.Prim(PrimitiveType.Sphere, transform, new Vector3(0, 0.62f, 0), Vector3.one * 0.34f, Fx.Additive(new Color(energy.r, energy.g, energy.b, 0.45f)), "Head", false);
            var h = GameObject.CreatePrimitive(PrimitiveType.Quad); h.name = "Halo"; Destroy(h.GetComponent<Collider>());
            h.transform.SetParent(transform, false); h.transform.localScale = Vector3.one * 4f;
            h.GetComponent<Renderer>().sharedMaterial = Fx.Additive(new Color(energy.r, energy.g, energy.b, 0.22f)); halo = h.transform;

            var pgo = new GameObject("Particles"); pgo.transform.SetParent(transform, false);
            ps = pgo.AddComponent<ParticleSystem>();
            var main = ps.main; main.startLifetime = 1.3f; main.startSpeed = 0.25f; main.startSize = 0.16f;
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.startColor = new Color(energy.r, energy.g, energy.b, 0.8f); main.maxParticles = 200;
            var em = ps.emission; em.rateOverTime = reduce ? 12 : 45;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.55f;
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(energy, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(0.9f, 0.2f), new GradientAlphaKey(0, 1) });
            col.color = new ParticleSystem.MinMaxGradient(g);
            pgo.GetComponent<ParticleSystemRenderer>().sharedMaterial = Fx.Additive(Color.white);

            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.7f; trail.startWidth = 0.5f; trail.endWidth = 0f; trail.sharedMaterial = Fx.Additive(new Color(energy.r, energy.g, energy.b, 0.5f));
            trail.emitting = !reduce;

            lamp = new GameObject("Light").AddComponent<Light>(); lamp.transform.SetParent(transform, false);
            lamp.type = LightType.Point; lamp.color = energy; lamp.range = reduce ? 6 : 14; lamp.intensity = reduce ? 0.8f : 1.8f;
            renders = GetComponentsInChildren<Renderer>(true);
        }
        void LateUpdate()
        {
            if (halo != null && Camera.main != null) halo.rotation = Camera.main.transform.rotation;
            float p = 1f + 0.05f * Mathf.Sin(Time.time * 3f); transform.localScale = Vector3.one * p;
        }
        public void SetVisible(bool v) { if (renders != null) foreach (var r in renders) if (r != null) r.enabled = v; if (trail != null) trail.emitting = v && !reduce; }
    }
}
