using UnityEngine;

namespace VARG.Player
{
    public class CaptureFeedbackFX : MonoBehaviour
    {
        private static CaptureFeedbackFX _instance;
        private ParticleSystem _particleSystem;

        public static CaptureFeedbackFX Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<CaptureFeedbackFX>();
                    if (_instance == null)
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
            _particleSystem = gameObject.AddComponent<ParticleSystem>();
            var renderer = GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                renderer.sharedMaterial = new Material(shader);
            }

            var main = _particleSystem.main;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.35f;
            main.startSpeed = 3f;

            var emission = _particleSystem.emission;
            emission.enabled = false;
        }

        public void PlayImpactSparks(Vector3 position, Color color, int count = 10)
        {
            EmitBurst(position, color, 0.1f, count);
        }

        public void PlayCapturePoof(Vector3 position, Color color, int count = 20)
        {
            EmitBurst(position, color, 0.25f, count);
        }

        private void EmitBurst(Vector3 position, Color color, float size, int count)
        {
            if (_particleSystem == null) return;

            var emitParams = new ParticleSystem.EmitParams
            {
                position = position,
                startColor = color,
                startSize = size,
                applyShapeToPosition = true
            };

            _particleSystem.Emit(emitParams, count);
        }
    }
}
