using System.Collections.Generic;
using System.Linq;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventSources
{
    public static class UnorderedEventLogRepositoryGenericExtensions
    {
        public static EventsQueryResult<TEvent, long?> GetEvents<TEvent>(
            this IUnorderedEventLogRepositoryGeneric<TEvent> unorderedEventLog, string type, long fromOffsetExclusive, long toOffsetInclusive, int estimatedCount)
            where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            var resultEvents = new List<TEvent>();
            foreach (var @event in unorderedEventLog.GetEvents(type, fromOffsetExclusive, toOffsetInclusive, estimatedCount + 10))
            {
                if (resultEvents.Count >= estimatedCount && resultEvents.Last().Ticks < @event.Ticks)
                    return new EventsQueryResult<TEvent, long?>(Split(resultEvents), resultEvents.Last().Ticks, noMoreEventsInSource : false);
                resultEvents.Add(@event);
            }
            for (var i = resultEvents.Count - 1; i > 0; i--)
            {
                if (resultEvents[i].Ticks > resultEvents[i - 1].Ticks)
                    return new EventsQueryResult<TEvent, long?>(Split(resultEvents.Take(i).ToList()), resultEvents[i - 1].Ticks, noMoreEventsInSource : false);
            }
            if (resultEvents.Count == 0)
                return new EventsQueryResult<TEvent, long?>(new List<EventWithOffset<TEvent, long?>>(), toOffsetInclusive, noMoreEventsInSource : true);
            return new EventsQueryResult<TEvent, long?>(Split(resultEvents), resultEvents.Last().Ticks, noMoreEventsInSource : true);
        }

        private static List<EventWithOffset<TEvent, long?>> Split<TEvent>(List<TEvent> events) where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            return events.SelectMany(x => x.SplitToElementary()).Select(x => new EventWithOffset<TEvent, long?>(x, x.Ticks)).ToList();
        }
    }
}