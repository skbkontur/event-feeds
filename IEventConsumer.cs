using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IEventConsumer<TEvent, TOffset>
    {
        [NotNull]
        string GetDescription();

        void ResetLocalState();

        [NotNull]
        EventsProcessingResult<TOffset> ProcessEvents([NotNull] EventsQueryResult<TEvent, TOffset> eventsQueryResult);
    }
}