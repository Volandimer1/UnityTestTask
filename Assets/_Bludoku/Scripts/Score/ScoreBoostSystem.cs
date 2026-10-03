using System;
using _Bludoku.Scripts.Combo;

namespace _Bludoku.Scripts.Score
{
    public class ScoreBoostSystem
    {
        private const int BoostCombo = 2;

        public event Action<bool> BoosterChanged;

        public bool IsBoosted { get; private set; }

        public void OnComboChanged(ComboChange change)
        {
            bool isBoosted = change.ComboCount >= BoostCombo;
            if (IsBoosted == isBoosted)
                return;

            IsBoosted = isBoosted;
            BoosterChanged?.Invoke(IsBoosted);
        }
    }
}
