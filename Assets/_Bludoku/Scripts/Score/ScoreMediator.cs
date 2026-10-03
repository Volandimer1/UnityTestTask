using _Bludoku.Scripts.Boards;
using UnityEngine;

namespace _Bludoku.Scripts.Score
{
    public class ScoreMediator : MonoBehaviour
    {
        [SerializeField] private ScoreView scoreView;
        [SerializeField] private Board board;
        [SerializeField] private ScoreBoosterView boosterView;
        
        private readonly ScoreBoostSystem _scoreBoostSystem = new();

        private void Awake()
        {
            board.OnFigurePlaced += FigurePlaced;
        }

        private void Start()
        {
            ScoreSystem.LoadScore();
            _scoreBoostSystem.Restore(ScoreSystem.ComboCount, ScoreSystem.ConsecutiveMisses);
            ScoreSystem.SetBoostProgress(_scoreBoostSystem.ComboCount, _scoreBoostSystem.ConsecutiveMisses,
                _scoreBoostSystem.IsBoosted);
            boosterView.SetBoosterEnabled(_scoreBoostSystem.IsBoosted);
            scoreView.UpdateScore(false);
        }

        public void ResetScore()
        {
            _scoreBoostSystem.Reset();
            ScoreSystem.ResetScore();
            UpdateView();
        }

        private void FigurePlaced(ClearResult result)
        {
            _scoreBoostSystem.FigurePlaced(result.ClearedCount);
            boosterView.SetBoosterEnabled(_scoreBoostSystem.IsBoosted);
            ScoreSystem.SetBoostProgress(_scoreBoostSystem.ComboCount, _scoreBoostSystem.ConsecutiveMisses,
                _scoreBoostSystem.IsBoosted);
            ScoreSystem.AddSetScore(result.ClearedCount);
            scoreView.UpdateScore();
        }

        private void UpdateView()
        {
            boosterView.SetBoosterEnabled(false);
            scoreView.UpdateScore(false);
        }
    }
}
