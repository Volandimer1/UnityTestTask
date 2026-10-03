using System.Collections.Generic;

namespace _Bludoku.Scripts.Analytics
{
    public sealed class AnalyticsEvent
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, object> Properties { get; }

        public AnalyticsEvent(string name, IReadOnlyDictionary<string, object> properties = null)
        {
            Name = name;
            Properties = properties;
        }

        public static AnalyticsEvent PieceMoved(int figureId, bool accepted)
        {
            return new AnalyticsEvent("piece_moved", new Dictionary<string, object>
            {
                ["figure_id"] = figureId,
                ["accepted"] = accepted
            });
        }

        public static AnalyticsEvent BonusReceived(string bonusId, int comboCount)
        {
            return new AnalyticsEvent("bonus_received", new Dictionary<string, object>
            {
                ["bonus_id"] = bonusId,
                ["combo_count"] = comboCount
            });
        }

        public static AnalyticsEvent PowerUpUsed(string powerUpId)
        {
            return new AnalyticsEvent("power_up_used", new Dictionary<string, object>
            {
                ["power_up_id"] = powerUpId
            });
        }
    }
}
