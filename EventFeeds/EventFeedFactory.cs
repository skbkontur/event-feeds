using JetBrains.Annotations;

using SkbKontur.EventFeeds.Building;
using SkbKontur.Graphite.Client;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public class EventFeedFactory
    {
        public EventFeedFactory(IGlobalTimeProvider globalTimeProvider, IGraphiteClient graphiteClient, IPeriodicJobRunner periodicJobRunner)
        {
            this.globalTimeProvider = globalTimeProvider;
            this.graphiteClient = graphiteClient;
            this.periodicJobRunner = periodicJobRunner;
        }

        [NotNull]
        public EventFeedsBuilder<TOffset> WithOffsetType<TOffset>()
        {
            return new EventFeedsBuilder<TOffset>(globalTimeProvider, graphiteClient, periodicJobRunner);
        }

        private readonly IGlobalTimeProvider globalTimeProvider;
        private readonly IGraphiteClient graphiteClient;
        private readonly IPeriodicJobRunner periodicJobRunner;
    }
}