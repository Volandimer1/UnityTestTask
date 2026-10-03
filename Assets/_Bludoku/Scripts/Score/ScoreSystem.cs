using UnityEngine;

namespace _Bludoku.Scripts.Score
{
    public static class ScoreSystem
    {
        private static int _score;
        private static int _highScore;
        
        private const string ScoreKey = "CurrentScore";
        private const string HighScoreKey = "HighScore";
        private const string BoosterKey = "Booster";
        private const string ComboCountKey = "ScoreComboCount";
        private const string ConsecutiveMissesKey = "ScoreComboMisses";
        private const int ScoreForSet = 1;
        private const float BoosterMultiplier = 1.5f;

        public static int Score => _score;
        public static int HighScore => _highScore;

        public static void LoadScore()
        {
            _score = PlayerPrefs.GetInt(ScoreKey, 0);
            _highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        public static void LoadComboProgress(out int comboCount, out int consecutiveMisses)
        {
            if (PlayerPrefs.HasKey(ComboCountKey) && PlayerPrefs.HasKey(ConsecutiveMissesKey))
            {
                comboCount = PlayerPrefs.GetInt(ComboCountKey);
                consecutiveMisses = PlayerPrefs.GetInt(ConsecutiveMissesKey);
            }
            else
            {
                // Existing saves only contain the booster flag. Restore the same minimum
                // combo state used before progress was persisted.
                comboCount = PlayerPrefs.GetInt(BoosterKey, 0) == 1 ? 2 : 0;
                consecutiveMisses = 0;
            }
        }

        public static void RecordPlacement(int clearedCount, int comboCount, int consecutiveMisses,
            bool boosterEnabled)
        {
            int scoreToAdd = clearedCount * ScoreForSet;
            scoreToAdd = (int)(scoreToAdd * (boosterEnabled ? BoosterMultiplier : 1));

            _score += scoreToAdd;
            if (_highScore < _score)
                _highScore = _score;

            SaveProgress(comboCount, consecutiveMisses, boosterEnabled);
        }

        public static void ResetScore()
        {
            _score = 0;
            SaveProgress(0, 0, false);
        }

        private static void SaveProgress(int comboCount, int consecutiveMisses, bool boosterEnabled)
        {
            PlayerPrefs.SetInt(ComboCountKey, comboCount);
            PlayerPrefs.SetInt(ConsecutiveMissesKey, consecutiveMisses);
            // Legacy saves may contain only this key; new saves derive booster state from combo progress.
            PlayerPrefs.SetInt(BoosterKey, boosterEnabled ? 1 : 0);
            PlayerPrefs.SetInt(ScoreKey, Score);
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
    }
}
