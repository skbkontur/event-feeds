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
        public List<EventWithOffset<TEvent, TOffset>> Events { get; private set; }

        [CanBeNull]
        public TOffset LastOffset { get; private set; }

        public bool NoMoreEventsInSource { get; private set; }

        public override string ToString()
        {
            return string.Format("Events.Count: {0}, LastOffset: {1}, NoMoreEventsInSource: {2}", Events.Count, LastOffset, NoMoreEventsInSource);
        }
    }
}