using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl.Interfaces;
using SKBKontur.Catalogue.Core.EventFeed.Providers.BusinessObjectStorage.Implementation;

namespace SKBKontur.Catalogue.Core.EventFeed.Providers.BusinessObjectStorage
{
    public static class EventLogEventSourceExtensions
    {
        public static IEventLogEventSource<T> Combine<T>(this IEventLogEventSource<T> a, IEventLogEventSource<T> b) where T : GenericEvent
        {
            return new CompositeSortedEventLogEventSource<T>(a, b);
        }
    }
}