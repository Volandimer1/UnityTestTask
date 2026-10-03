namespace _Bludoku.Scripts.Analytics
{
    public interface IAnalyticsService
    {
        void Track(AnalyticsEvent analyticsEvent);
    }
}
