using System;
using SKBKontur.Catalogue.Core.Configuration.Settings;

namespace SKBKontur.Catalogue.Core.EventFeeds.Firing
{
    public interface IEventFeedsSettings
    {
        TimeSpan ActualizeInterval { get; }
    }

    public class EventFeedsSettings : IEventFeedsSettings
    {
        public EventFeedsSettings(IApplicationSettings applicationSettings)
        {
            ActualizeInterval = applicationSettings.GetTimeSpan("EventFeeds.ActualizeInterval");
        }

        public TimeSpan ActualizeInterval { get; private set; }
    }
}