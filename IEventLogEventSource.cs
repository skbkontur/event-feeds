using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl.Interfaces
{
    public interface IEventLogEventSource<TEvent>
    {
        [NotNull]
        string GetDescription();

        [NotNull]
        EventsQueryResult<TEvent, long> GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int estimatedCount);
    }
}