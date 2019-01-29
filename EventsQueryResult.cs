using System.Collections.Generic;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public class EventsQueryResult<TEvent, TOffset>
    {
        public EventsQueryResult([NotNull] List<EventWithOffset<TEvent, TOffset>> events, [CanBeNull] TOffset lastOffset, bool noMoreEventsInSource)
        {
            Events = events;
            LastOffset = lastOffset;
            NoMoreEventsInSource = noMoreEventsInSource;
        }

        [NotNull]
        public List<EventWithOffset<TEvent, TOffset>> Events { get; }

        [CanBeNull]
        public TOffset LastOffset { get; }

        public bool NoMoreEventsInSource { get; }

        public override string ToString()
        {
            return $"Events.Count: {Events.Count}, LastOffset: {LastOffset}, NoMoreEventsInSource: {NoMoreEventsInSource}";
        }
    }
}