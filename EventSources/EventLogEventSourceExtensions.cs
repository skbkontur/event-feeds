using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventSources
{
    public static class EventLogEventSourceExtensions
    {
        public static IEventSource<T> Combine<T>(this IEventSource<T> a, IEventSource<T> b) where T : GenericEvent
        {
            return new CompositeSortedEventSource<T>(a, b);
        }
    }
}