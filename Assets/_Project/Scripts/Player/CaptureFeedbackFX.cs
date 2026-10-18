using UnityEngine;

namespace VARG.Player
{
    public class CaptureFeedbackFX : MonoBehaviour
    {
        private static CaptureFeedbackFX _instance;

        private ParticleSystem _impactParticleSystem;
        private ParticleSystem _capturePoofParticleSystem;

        private static Material _particleMaterial;
        private static Texture2D _softParticleTexture;

        public static CaptureFeedbackFX Instance
        {
            get
            {
                if (_instance == null)
                {
                    var existing = Object.FindFirstObjectByType<CaptureFeedbackFX>();
                    if (existing != null)
                    {
                        _instance = existing;
                    }
                    else
                    {
                        var go = new GameObject("[CaptureFeedbackFX]");
                        _instance = go.AddComponent<CaptureFeedbackFX>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            InitializeParticles();
        }

        private void InitializeParticles()
        {
            EnsureMaterial();

            _impactParticleSystem = CreateParticleSystem("ImpactSparks", 60);
            ConfigureImpactSparks(_impactParticleSystem);

            _capturePoofParticleSystem = CreateParticleSystem("CapturePoof", 120);
            ConfigureCapturePoof(_capturePoofParticleSystem);
        }

        private static void EnsureMaterial()
        {
            if (_particleMaterial != null) return;

            if (_softParticleTexture == null)
            {
                _softParticleTexture = GenerateSoftCircleTexture(32);
            }

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                      ?? Shader.Find("Particles/Standard Unlit")
                      ?? Shader.Find("Sprites/Default")
                      ?? Shader.Find("Unlit/Color");

            _particleMaterial = new Material(shader)
            {
                mainTexture = _softParticleTexture
            };
        }

        private static Texture2D GenerateSoftCircleTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "Procedural_SoftCircle"
            };

            var center = (size - 1) * 0.5f;
            var radius = center;
            var pixels = new Color[size * size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    var normDist = dist / radius;
                    var alpha = Mathf.Clamp01(1f - normDist);
                    alpha = Mathf.Pow(alpha, 1.8f);

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private ParticleSystem CreateParticleSystem(string systemName, int maxParticles)
        {
            var childGo = new GameObject(systemName);
            childGo.transform.SetParent(transform, false);

            var ps = childGo.AddComponent<ParticleSystem>();
            var renderer = childGo.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = _particleMaterial;

            var main = ps.main;
            main.playOnAwake = false;
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.enabled = false;

            return ps;
        }

        private static void ConfigureImpactSparks(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = 0.2f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
            main.startColor = new Color(0.2f, 1f, 0.5f, 0.9f);
            main.gravityModifier = 0.5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;
        }

        private static void ConfigureCapturePoof(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startColor = new Color(0.9f, 0.95f, 1f, 0.85f);
            main.gravityModifier = -0.2f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;
        }

        public void PlayImpactSparks(Vector3 position, Color color, int count = 12)
        {
            if (_impactParticleSystem == null) return;

            var emitParams = new ParticleSystem.EmitParams
            {
                position = position,
                startColor = color,
                applyShapeToPosition = true
            };

            _impactParticleSystem.Emit(emitParams, count);
        }

        public void PlayCapturePoof(Vector3 position, Color color, int count = 24)
        {
            if (_capturePoofParticleSystem == null) return;

            var emitParams = new ParticleSystem.EmitParams
            {
                position = position,
                startColor = color,
                applyShapeToPosition = true
            };

            _capturePoofParticleSystem.Emit(emitParams, count);
        }
    }
}
