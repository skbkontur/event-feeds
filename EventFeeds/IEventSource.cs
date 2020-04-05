using JetBrains.Annotations;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public interface IEventSource<TEvent, TOffset>
    {
        [NotNull]
        string GetDescription();

        [NotNull]
        EventsQueryResult<TEvent, TOffset> GetEvents([CanBeNull] TOffset fromOffsetExclusive, [NotNull] TOffset toOffsetInclusive, int estimatedCount);
    }
}