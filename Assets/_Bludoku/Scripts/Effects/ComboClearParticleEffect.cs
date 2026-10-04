using System;
using _Bludoku.Scripts.Boards;
using UnityEngine;

namespace _Bludoku.Scripts.Effects
{
    // Extra feedback is pooled and emitted after the mediator has updated combo state.
    // The original clear particle prefab continues to play independently.
    public sealed class ComboClearParticleEffect : IDisposable
    {
        private readonly ParticleSystem _stars;
        private readonly ParticleSystem _flames;
        private readonly Material _starMaterial;
        private readonly Material _flameMaterial;

        public ComboClearParticleEffect(Transform parent)
        {
            Shader shader = ComboVfxAssets.AdditiveShader;
            if (shader == null)
            {
                Debug.LogError("Combo VFX shader is missing from Resources/ComboVfx.");
                return;
            }
            _starMaterial = new Material(shader) { mainTexture = ComboVfxAssets.Star };
            _flameMaterial = new Material(shader) { mainTexture = ComboVfxAssets.Flame };
            _stars = CreateSystem("ComboStars", parent, _starMaterial);
            _flames = CreateSystem("ComboFlames", parent, _flameMaterial);
        }

        public void Play(ClearResult result, int comboCount)
        {
            if (result.ClearedCount == 0 || _stars == null || comboCount < ComboVfxStages.Electric)
                return;

            bool fire = comboCount >= ComboVfxStages.FireCount;
            ParticleSystem system = fire ? _flames : _stars;
            system.Play();
            int stride = Mathf.Max(1, Mathf.CeilToInt(result.ClearedPositions.Count / 32f));
            for (int i = 0; i < result.ClearedPositions.Count; i += stride)
            {
                Vector3 position = result.ClearedPositions[i];
                position.z -= 0.2f;
                for (int n = 0; n < 3; n++)
                {
                    Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
                    var particle = new ParticleSystem.EmitParams
                    {
                        position = position,
                        velocity = n == 0 ? Vector3.zero :
                            new Vector3(direction.x, direction.y, 0f) * (fire ? 1.4f : 1.8f),
                        startLifetime = (n == 0 ? 0.38f : fire ? 1.1f : 0.85f) * 1.2f,
                        startSize = n == 0 ? (fire ? 1.4f : 1.2f) :
                            (fire ? 0.9f : 0.7f),
                        startColor = fire ? new Color(1f, 0.85f, 0.45f, 1f) :
                            new Color(0.65f, 0.95f, 1f, 1f)
                    };
                    system.Emit(particle, 1);
                }
            }
        }

        public void Dispose()
        {
            if (_starMaterial != null)
                UnityEngine.Object.Destroy(_starMaterial);
            if (_flameMaterial != null)
                UnityEngine.Object.Destroy(_flameMaterial);
        }

        private static ParticleSystem CreateSystem(string name, Transform parent, Material material)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.SetParent(parent, false);
            var system = go.AddComponent<ParticleSystem>();
            var main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 256;
            main.startSpeed = 0f;
            main.startSize = 0.3f;
            main.startLifetime = 0.5f;
            var emission = system.emission;
            emission.enabled = false;
            var shape = system.shape;
            shape.enabled = false;
            var color = system.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.85f, 0.55f),
                    new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(0.15f, 1f),
                    new Keyframe(0.6f, 0.85f), new Keyframe(1f, 0f)));
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 1200;
            renderer.sharedMaterial = material;
            go.SetActive(true);
            return system;
        }
    }
}
