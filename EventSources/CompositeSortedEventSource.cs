using System;
using System.Linq;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventSources
{
    internal class CompositeSortedEventSource<T> : IEventSource<T> where T : GenericEvent
    {
        public CompositeSortedEventSource(IEventSource<T> eventSource1, IEventSource<T> eventSource2)
        {
            this.eventSource1 = eventSource1;
            this.eventSource2 = eventSource2;
        }

        public string GetDescription()
        {
            return string.Format("Two merged sources: 1: [{0}], 2: [{1}]", eventSource1.GetDescription(), eventSource2.GetDescription());
        }

        public EventsQueryResult<T, long> GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int estimatedCount)
        {
            var res1 = eventSource1.GetEvents(fromOffsetExclusive, toOffsetInclusive, estimatedCount);
            var res2 = eventSource2.GetEvents(fromOffsetExclusive, toOffsetInclusive, estimatedCount);
            if(res1.NoMoreEventsInSource)
                return res2;
            if(res2.NoMoreEventsInSource)
                return res1;
            return new EventsQueryResult<T, long>(res1.Events.Concat(res2.Events).ToList(), Math.Min(res1.LastOffset, res2.LastOffset), false);
        }

        private readonly IEventSource<T> eventSource1;
        private readonly IEventSource<T> eventSource2;
    }
}