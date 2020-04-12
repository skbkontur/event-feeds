using System;
using System.Collections.Concurrent;
using System.Linq;

using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;

namespace EventFeeds.Tests
{
    public class TestEventSource : IEventSource<int, long?>
    {
        public ConcurrentBag<EventWithOffset<int, long?>> Timeline { get; } = new ConcurrentBag<EventWithOffset<int, long?>>();

        public string GetDescription()
        {
            return GetType().Name;
        }

        public void WriteEvent(Timestamp timestamp, int value)
        {
            Timeline.Add(new EventWithOffset<int, long?>(value, timestamp.Ticks));
        }

        public EventsQueryResult<int, long?> GetEvents(long? fromOffsetExclusive, long? toOffsetInclusive, int estimatedCount)
        {
            if (toOffsetInclusive == null)
                throw new InvalidOperationException("toOffsetInclusive is null");

            var events = Timeline.OrderBy(x => x.Offset)
                                 .Where(x => x.Offset > (fromOffsetExclusive ?? 0) && x.Offset <= toOffsetInclusive)
                                 .ToList();

            var lastOffset = events.Any() ? events.Last().Offset : fromOffsetExclusive;
            return new EventsQueryResult<int, long?>(events, lastOffset, noMoreEventsInSource : true);
        }
    }
}