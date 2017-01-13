using System.Collections.Generic;
using System.Linq;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventSources
{
    public static class UnorderedEventLogRepositoryGenericExtensions
    {
        public static EventsQueryResult<TEvent, long> GetEvents<TEvent>(
            this IUnorderedEventLogRepositoryGeneric<TEvent> unorderedEventLog, string type, long fromOffsetExclusive, long toOffsetInclusive, int estimatedCount)
            where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            var resultEvents = new List<TEvent>();
            foreach(var @event in unorderedEventLog.GetEvents(type, fromOffsetExclusive, toOffsetInclusive, estimatedCount + 10))
            {
                if(resultEvents.Count >= estimatedCount && resultEvents.Last().Ticks < @event.Ticks)
                    return new EventsQueryResult<TEvent, long>(Split(resultEvents), resultEvents.Last().Ticks, false);
                resultEvents.Add(@event);
            }
            for(var i = resultEvents.Count - 1; i > 0; i--)
            {
                if(resultEvents[i].Ticks > resultEvents[i - 1].Ticks)
                    return new EventsQueryResult<TEvent, long>(Split(resultEvents.Take(i).ToList()), resultEvents[i - 1].Ticks, false);
            }
            if(resultEvents.Count == 0)
                return new EventsQueryResult<TEvent, long>(resultEvents, fromOffsetExclusive, true);
            return new EventsQueryResult<TEvent, long>(Split(resultEvents), resultEvents.Last().Ticks, true);
        }

        private static List<TEvent> Split<TEvent>(List<TEvent> events) where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            return events.SelectMany(x => x.SplitToElementary()).ToList();
        }
    }
}