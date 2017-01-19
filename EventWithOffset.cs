using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public class EventWithOffset<TEvent, TOffset>
    {
        public EventWithOffset([NotNull] TEvent @event, [NotNull] TOffset offset)
        {
            if(offset == null)
                throw new InvalidProgramStateException(string.Format("Offset is null for event: {0}", @event));
            Event = @event;
            Offset = offset;
        }

        [NotNull]
        public TEvent Event { get; private set; }

        [NotNull]
        public TOffset Offset { get; private set; }
    }
}