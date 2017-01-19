using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public class EventsQueryResult<TEvent, TOffset>
    {
        public EventsQueryResult([NotNull] List<EventWithOffset<TEvent, TOffset>> events, [NotNull] TOffset lastOffset, bool noMoreEventsInSource)
        {
            if(lastOffset == null)
                throw new InvalidProgramStateException("LastOffset is null");
            Events = events;
            LastOffset = lastOffset;
            NoMoreEventsInSource = noMoreEventsInSource;
        }

        [NotNull]
        public List<EventWithOffset<TEvent, TOffset>> Events { get; private set; }

        [NotNull]
        public TOffset LastOffset { get; private set; }

        public bool NoMoreEventsInSource { get; private set; }
    }
}