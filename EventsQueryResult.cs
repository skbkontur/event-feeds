using System.Collections.Generic;
using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public class EventsQueryResult<TEvent, TOffset>
    {
        public EventsQueryResult([NotNull] List<TEvent> events, [NotNull] TOffset lastOffset, bool noMoreEventsInSource)
        {
            Events = events;
            LastOffset = lastOffset;
            NoMoreEventsInSource = noMoreEventsInSource;
        }

        [NotNull]
        public List<TEvent> Events { get; private set; }

        [NotNull]
        public TOffset LastOffset { get; private set; }

        public bool NoMoreEventsInSource { get; private set; }
    }
}