using System;

namespace _Bludoku.Scripts.Combo
{
    public class ComboSystem
    {
        private const int ConsecutiveMissesToBreakCombo = 3;

        private int _comboCount;
        private int _consecutiveMisses;

        // Raised immediately after each state update. Placement events precede scoring and saving.
        public event Action<ComboChange> ComboChanged;

        public int ComboCount => _comboCount;
        public int ConsecutiveMisses => _consecutiveMisses;

        public void Restore(int comboCount, int consecutiveMisses)
        {
            _comboCount = Math.Max(0, comboCount);
            _consecutiveMisses = Math.Max(0, consecutiveMisses);

            if (_consecutiveMisses >= ConsecutiveMissesToBreakCombo)
                _comboCount = 0;

            Publish(ComboChangeKind.Restored);
        }

        public void Reset()
        {
            _comboCount = 0;
            _consecutiveMisses = 0;
            Publish(ComboChangeKind.Reset);
        }

        public void RecordSuccessfulAction()
        {
            _consecutiveMisses = 0;
            _comboCount++;
            Publish(ComboChangeKind.SuccessfulAction);
        }

        public void RecordUnsuccessfulAction()
        {
            _consecutiveMisses++;
            if (_consecutiveMisses >= ConsecutiveMissesToBreakCombo)
                _comboCount = 0;

            Publish(ComboChangeKind.UnsuccessfulAction);
        }

        private void Publish(ComboChangeKind kind)
        {
            ComboChanged?.Invoke(new ComboChange(_comboCount, _consecutiveMisses, kind));
        }
    }
}
