using System.Text;
using UnityEngine;

namespace _Bludoku.Scripts.Analytics
{
    public sealed class ConsoleAnalyticsService : IAnalyticsService
    {
        public void Track(AnalyticsEvent analyticsEvent)
        {
            var message = new StringBuilder("[Analytics] ").Append(analyticsEvent.Name);

            if (analyticsEvent.Properties != null)
            {
                foreach (var property in analyticsEvent.Properties)
                    message.Append(' ').Append(property.Key).Append('=').Append(property.Value);
            }

            Debug.Log(message.ToString());
        }
    }
}
