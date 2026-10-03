using System;
using _Bludoku.Scripts.Boards;
using _Bludoku.Scripts.Combo;
using UnityEngine;

namespace _Bludoku.Scripts.Score
{
    public class ScoreMediator : MonoBehaviour
    {
        public event Action<int> BoosterActivated;

        [SerializeField] private ScoreView scoreView;
        [SerializeField] private Board board;
        [SerializeField] private ScoreBoosterView boosterView;

        private ComboSystem _comboSystem;
        private readonly ScoreBoostSystem _scoreBoostSystem = new();

        public void Bind(ComboSystem comboSystem)
        {
            _comboSystem = comboSystem;
            _comboSystem.ComboChanged += _scoreBoostSystem.OnComboChanged;
            _scoreBoostSystem.BoosterChanged += boosterView.SetBoosterEnabled;
            board.OnFigurePlaced += OnFigurePlaced;
        }

        private void OnDestroy()
        {
            if (_comboSystem == null)
                return;

            board.OnFigurePlaced -= OnFigurePlaced;
            _scoreBoostSystem.BoosterChanged -= boosterView.SetBoosterEnabled;
            _comboSystem.ComboChanged -= _scoreBoostSystem.OnComboChanged;
        }

        public void RestoreProgress()
        {
            ScoreSystem.LoadScore();
            ScoreSystem.LoadComboProgress(out int comboCount, out int consecutiveMisses);
            _comboSystem.Restore(comboCount, consecutiveMisses);
            scoreView.UpdateScore(false);
        }

        public void ResetScore()
        {
            _comboSystem.Reset();
            ScoreSystem.ResetScore();
            scoreView.UpdateScore(false);
        }

        private void OnFigurePlaced(ClearResult result)
        {
            bool wasBoosted = _scoreBoostSystem.IsBoosted;

            if (result.ClearedCount > 0)
                _comboSystem.RecordSuccessfulAction();
            else
                _comboSystem.RecordUnsuccessfulAction();

            ScoreSystem.RecordPlacement(result.ClearedCount, _comboSystem.ComboCount,
                _comboSystem.ConsecutiveMisses, _scoreBoostSystem.IsBoosted);
            scoreView.UpdateScore();

            if (!wasBoosted && _scoreBoostSystem.IsBoosted)
                BoosterActivated?.Invoke(_comboSystem.ComboCount);
        }
    }
}
