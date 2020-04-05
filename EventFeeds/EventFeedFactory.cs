using JetBrains.Annotations;

using SkbKontur.EventFeeds.Building;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public class EventFeedFactory
    {
        public EventFeedFactory(IGlobalTimeProvider globalTimeProvider, IPeriodicJobRunner periodicJobRunner)
        {
            this.globalTimeProvider = globalTimeProvider;
            this.periodicJobRunner = periodicJobRunner;
        }

        [NotNull]
        public EventFeedsBuilder<TOffset> WithOffsetType<TOffset>()
        {
            return new EventFeedsBuilder<TOffset>(globalTimeProvider, periodicJobRunner);
        }

        private readonly IGlobalTimeProvider globalTimeProvider;
        private readonly IPeriodicJobRunner periodicJobRunner;
    }
}