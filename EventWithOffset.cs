using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public class EventWithOffset<TEvent, TOffset>
    {
        public EventWithOffset([NotNull] TEvent @event, [NotNull] TOffset offset)
        {
            if (offset == null)
                throw new InvalidProgramStateException($"Offset is null for event: {@event}");
            Event = @event;
            Offset = offset;
        }

        [NotNull]
        public TEvent Event { get; }

        [NotNull]
        public TOffset Offset { get; }

        public override string ToString()
        {
            return $"Event: {Event}, Offset: {Offset}";
        }
    }
}