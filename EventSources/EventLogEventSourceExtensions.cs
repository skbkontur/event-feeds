using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventSources
{
    public static class EventLogEventSourceExtensions
    {
        public static IEventLogEventSource<T> Combine<T>(this IEventLogEventSource<T> a, IEventLogEventSource<T> b) where T : GenericEvent
        {
            return new CompositeSortedEventLogEventSource<T>(a, b);
        }
    }
}