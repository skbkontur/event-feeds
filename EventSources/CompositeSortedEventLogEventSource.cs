using System;
using System.Linq;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventSources
{
    internal class CompositeSortedEventLogEventSource<T> : IEventLogEventSource<T> where T : GenericEvent
    {
        public CompositeSortedEventLogEventSource(IEventLogEventSource<T> eventLogEventSource1, IEventLogEventSource<T> eventLogEventSource2)
        {
            this.eventLogEventSource1 = eventLogEventSource1;
            this.eventLogEventSource2 = eventLogEventSource2;
        }

        public string GetDescription()
        {
            return string.Format("Two merged sources: 1: [{0}], 2: [{1}]", eventLogEventSource1.GetDescription(), eventLogEventSource2.GetDescription());
        }

        public EventsQueryResult<T, long> GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int estimatedCount)
        {
            var res1 = eventLogEventSource1.GetEvents(fromOffsetExclusive, toOffsetInclusive, estimatedCount);
            var res2 = eventLogEventSource2.GetEvents(fromOffsetExclusive, toOffsetInclusive, estimatedCount);
            if(res1.NoMoreEventsInSource)
                return res2;
            if(res2.NoMoreEventsInSource)
                return res1;
            return new EventsQueryResult<T, long>(res1.Events.Concat(res2.Events).ToList(), Math.Min(res1.LastOffset, res2.LastOffset), false);
        }

        private readonly IEventLogEventSource<T> eventLogEventSource1;
        private readonly IEventLogEventSource<T> eventLogEventSource2;
    }
}