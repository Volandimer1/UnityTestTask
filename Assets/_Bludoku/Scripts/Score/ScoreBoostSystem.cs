using System;

namespace _Bludoku.Scripts.Score
{
    public class ScoreBoostSystem
    {
        private int _movesCount;
        private int _comboCount;
        
        private const int MovesThreshold = 3;
        private const int BoostCombo = 2;
        
        public int ComboCount => _comboCount;
        public int ConsecutiveMisses => _movesCount;
        public bool IsBoosted => _comboCount >= BoostCombo;

        public void Restore(int comboCount, int consecutiveMisses)
        {
            _comboCount = Math.Max(0, comboCount);
            _movesCount = Math.Max(0, consecutiveMisses);

            if (_movesCount >= MovesThreshold)
                _comboCount = 0;
        }

        public void Reset()
        {
            _comboCount = 0;
            _movesCount = 0;
        }

        public void FigurePlaced(int removes)
        {
            if (removes == 0)
            {
                _movesCount++;
            }
            else
            {
                _movesCount = 0;
                _comboCount++;
            }
            
            if (_movesCount >= MovesThreshold)
            {
                _comboCount = 0;
            }
        }
    }
}
