using System;

using JetBrains.Annotations;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public class EventWithOffset<TEvent, TOffset>
    {
        public EventWithOffset([NotNull] TEvent @event, [NotNull] TOffset offset)
        {
            if (offset == null)
                throw new InvalidOperationException($"Offset is null for event: {@event}");
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