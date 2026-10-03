using UnityEngine;

namespace _Bludoku.Scripts.Score
{
    public static class ScoreSystem
    {
        private static int _score;
        private static int _highScore;
        private static bool _isBoosterEnabled;
        private static int _comboCount;
        private static int _consecutiveMisses;
        
        private const string ScoreKey = "CurrentScore";
        private const string HighScoreKey = "HighScore";
        private const string BoosterKey = "Booster";
        private const string ComboCountKey = "ScoreComboCount";
        private const string ConsecutiveMissesKey = "ScoreComboMisses";
        private const int ScoreForSet = 1;
        private const float BoosterMultiplier = 1.5f;

        public static int Score => _score;
        public static int HighScore => _highScore;
        public static bool IsBoosterEnabled => _isBoosterEnabled;
        public static int ComboCount => _comboCount;
        public static int ConsecutiveMisses => _consecutiveMisses;

        public static void SetBoostProgress(int comboCount, int consecutiveMisses, bool boosterEnabled)
        {
            _comboCount = comboCount;
            _consecutiveMisses = consecutiveMisses;
            _isBoosterEnabled = boosterEnabled;
        }

        public static void LoadScore()
        {
            _score = PlayerPrefs.GetInt(ScoreKey, 0);
            _highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            _isBoosterEnabled = PlayerPrefs.GetInt(BoosterKey) == 1;

            if (PlayerPrefs.HasKey(ComboCountKey) && PlayerPrefs.HasKey(ConsecutiveMissesKey))
            {
                _comboCount = PlayerPrefs.GetInt(ComboCountKey);
                _consecutiveMisses = PlayerPrefs.GetInt(ConsecutiveMissesKey);
            }
            else
            {
                // Existing saves only contain the booster flag. Restore the same minimum
                // combo state that ScoreBoostSystem used before progress was persisted.
                _comboCount = _isBoosterEnabled ? 2 : 0;
                _consecutiveMisses = 0;
            }
        }
        
        public static void AddSetScore(int setsCount)
        {
            int scoreToAdd = setsCount * ScoreForSet;
            scoreToAdd = (int)(scoreToAdd * (IsBoosterEnabled ? BoosterMultiplier : 1));
            
            AddScore(scoreToAdd);
        }
        
        public static void AddScore(int score)
        {
            _score = Score + score;
            if (HighScore < Score)
            {
                _highScore = Score;
            }
            
            SaveScore();
        }

        public static void ResetScore()
        {
            _score = 0;
            _isBoosterEnabled = false;
            _comboCount = 0;
            _consecutiveMisses = 0;
            SaveScore();
        }

        private static void SaveScore()
        {
            PlayerPrefs.SetInt(BoosterKey, IsBoosterEnabled ? 1 : 0);
            PlayerPrefs.SetInt(ComboCountKey, ComboCount);
            PlayerPrefs.SetInt(ConsecutiveMissesKey, ConsecutiveMisses);
            PlayerPrefs.SetInt(ScoreKey, Score);
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
    }
}
