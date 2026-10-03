namespace _Bludoku.Scripts.Combo
{
    public enum ComboChangeKind
    {
        Restored,
        Reset,
        SuccessfulAction,
        UnsuccessfulAction
    }

    public readonly struct ComboChange
    {
        public int ComboCount { get; }
        public int ConsecutiveMisses { get; }
        public ComboChangeKind Kind { get; }

        public ComboChange(int comboCount, int consecutiveMisses, ComboChangeKind kind)
        {
            ComboCount = comboCount;
            ConsecutiveMisses = consecutiveMisses;
            Kind = kind;
        }
    }
}
