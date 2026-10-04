using _Bludoku.Scripts.Boards;
using UnityEngine;

namespace _Bludoku.Scripts.Effects
{
    public class EffectsManager : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private ParticleSystem particles;

        private ParticleEffect _particleEffect;
        private VibrationEffect _vibrationEffect;
        private ComboClearParticleEffect _comboClearParticles;
        
        private void Awake()
        {
            _particleEffect = new ParticleEffect(particles);
            _vibrationEffect = new VibrationEffect();
            _comboClearParticles = new ComboClearParticleEffect(transform);
            
            board.OnFigurePlaced += OnFigurePlaced;
        }

        private void OnFigurePlaced(ClearResult result)
        {
            _vibrationEffect.Play(result);
            _particleEffect.Play(result);
        }

        public void PlayComboClearParticles(ClearResult result, int comboCount)
        {
            _comboClearParticles.Play(result, comboCount);
        }

        private void OnDestroy()
        {
            board.OnFigurePlaced -= OnFigurePlaced;
            _comboClearParticles?.Dispose();
        }
    }
}
