using JetBrains.Annotations;

using SkbKontur.Cassandra.GlobalTimestamp;

using SKBKontur.Catalogue.Core.EventFeeds.Building;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

using SkbKontur.Graphite.Client;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    [PublicAPI]
    public class EventFeedFactory
    {
        public EventFeedFactory(IGlobalTime defaultGlobalTime,
                                IGraphiteClient graphiteClient,
                                IPeriodicTaskRunner periodicTaskRunner,
                                IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.defaultGlobalTime = defaultGlobalTime;
            this.graphiteClient = graphiteClient;
            this.periodicTaskRunner = periodicTaskRunner;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
        }

        [NotNull]
        public EventFeedsBuilder<TOffset> WithOffsetType<TOffset>()
        {
            return new EventFeedsBuilder<TOffset>(defaultGlobalTime, graphiteClient, periodicTaskRunner, periodicJobRunnerWithLeaderElection);
        }

        private readonly IGlobalTime defaultGlobalTime;
        private readonly IGraphiteClient graphiteClient;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
    }
}