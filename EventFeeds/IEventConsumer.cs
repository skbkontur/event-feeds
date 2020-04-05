using JetBrains.Annotations;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public interface IEventConsumer<TEvent, TOffset>
    {
        [NotNull]
        string GetDescription();

        void ResetLocalState();

        [NotNull]
        EventsProcessingResult<TOffset> ProcessEvents([NotNull] EventsQueryResult<TEvent, TOffset> eventsQueryResult);
    }
}