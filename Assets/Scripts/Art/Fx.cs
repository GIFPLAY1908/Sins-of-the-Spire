using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>One-shot particle bursts and small transient effects.</summary>
    public static class Fx
    {
        private static Material _particleMaterial;
        private static Material _glowParticleMaterial;

        private static Material ParticleMaterial
        {
            get
            {
                if (_particleMaterial != null) return _particleMaterial;
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                _particleMaterial = new Material(shader) { name = "FxParticle" };
                if (_particleMaterial.HasProperty("_BaseColor")) _particleMaterial.SetColor("_BaseColor", Color.white);
                if (_particleMaterial.HasProperty("_Color")) _particleMaterial.SetColor("_Color", Color.white);
                return _particleMaterial;
            }
        }

        public static void Burst(Vector3 position, Color color, int count = 14, float speed = 4.5f, float size = 0.16f,
            float lifetime = 0.45f, float gravity = 0.6f)
        {
            var go = new GameObject("FxBurst");
            go.transform.position = position;

            var system = go.AddComponent<ParticleSystem>();
            var main = system.main;
            main.duration = 0.2f;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.maxParticles = 200;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var velocityOverLifetime = system.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(0f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = ParticleMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            system.Play();
            Object.Destroy(go, lifetime + 1.2f);
        }

        public static void Impact(Vector3 position, Color color) => Burst(position, color, 8, 3.5f, 0.12f, 0.3f, 0.2f);

        public static void Death(Vector3 position, Color color) => Burst(position, color, 26, 6.5f, 0.2f, 0.7f, 0.8f);

        public static void Ring(Vector3 position, Color color, float radius)
        {
            var go = new GameObject("FxRing");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.1f;
            var ring = go.AddComponent<ShockRing>();
            ring.Initialise(color, radius, 0.6f);
        }

        public static void FloatingScore(Vector3 position, string text, Color color)
        {
            var go = new GameObject("FxText");
            go.transform.position = position + Vector3.up * 1.4f;

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.color = color;
            mesh.fontSize = 64;
            mesh.characterSize = 0.06f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;

            var renderer = go.GetComponent<MeshRenderer>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                mesh.font = font;
                if (renderer != null) renderer.sharedMaterial = font.material;
            }
            if (renderer != null) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (renderer != null) renderer.receiveShadows = false;

            go.AddComponent<FloatingText>().Initialise(0.9f);
        }
    }

    /// <summary>Expanding ground ring used for boss shockwaves.</summary>
    public class ShockRing : MonoBehaviour
    {
        private float _radius;
        private float _duration;
        private float _elapsed;
        private Material _material;

        public void Initialise(Color color, float radius, float duration)
        {
            _radius = radius;
            _duration = duration;
            _material = Palette.Glow(color, 2.2f);

            var disc = MeshKit.MeshPart(transform, MeshKit.Cone(1f, 0.98f, 0.02f, 28), "Disc",
                Vector3.zero, Vector3.one, _material);
            _ = disc;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            float scale = Mathf.Lerp(0.1f, _radius, 1f - (1f - t) * (1f - t));
            transform.localScale = new Vector3(scale, 1f, scale);
            if (t >= 1f) Destroy(gameObject);
        }
    }

    public class FloatingText : MonoBehaviour
    {
        private float _life;
        private float _elapsed;

        public void Initialise(float life) => _life = life;

        private void Update()
        {
            _elapsed += Time.deltaTime;
            transform.position += Vector3.up * (Time.deltaTime * 1.2f);
            var text = GetComponent<TextMesh>();
            if (text != null)
            {
                var color = text.color;
                color.a = Mathf.Clamp01(1f - _elapsed / _life);
                text.color = color;
            }

            if (_elapsed >= _life) Destroy(gameObject);
        }
    }
}
