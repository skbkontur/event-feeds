using System;

using SKBKontur.Catalogue.Core.Configuration.Settings;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl
{
    public interface IEventFeedsSettings
    {
        TimeSpan ActualizeInterval { get; }
        string UnprocessedEventsLocation { get; }
    }

    public class EventFeedsSettings : IEventFeedsSettings
    {
        public EventFeedsSettings(IApplicationSettings applicationSettings)
        {
            ActualizeInterval = applicationSettings.GetTimeSpan("EventFeeds.ActualizeInterval");
            UnprocessedEventsLocation = applicationSettings.GetString("EventFeeds.UnprocessedEventsLocation");
        }

        public TimeSpan ActualizeInterval { get; private set; }
        public string UnprocessedEventsLocation { get; private set; }
    }
}