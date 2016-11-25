using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IEventSource<TEvent>
    {
        [NotNull]
        string GetDescription();

        [NotNull]
        EventsQueryResult<TEvent, long> GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int estimatedCount);
    }
}