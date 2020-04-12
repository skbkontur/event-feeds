using System.Collections.Concurrent;
using System.Linq;

using SkbKontur.EventFeeds;

namespace EventFeeds.Tests
{
    public class TestEventConsumer : IEventConsumer<int, long?>
    {
        public ConcurrentBag<EventWithOffset<int, long?>> ConsumedEvents { get; } = new ConcurrentBag<EventWithOffset<int, long?>>();

        public string GetDescription()
        {
            return GetType().Name;
        }

        public void ResetLocalState()
        {
        }

        public EventsProcessingResult<long?> ProcessEvents(EventsQueryResult<int, long?> eventsQueryResult)
        {
            foreach (var eventWithOffset in eventsQueryResult.Events)
                ConsumedEvents.Add(eventWithOffset);

            return eventsQueryResult.Events.Any()
                       ? EventsProcessingResult<long?>.DoCommitOffset(eventsQueryResult.Events.Last().Offset)
                       : EventsProcessingResult<long?>.DoNotCommitOffset();
        }
    }
}