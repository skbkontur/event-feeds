using System;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class ConsequentEventFeed<TEvent, TOffset> : CompositeEventFeedBase<TEvent, TOffset>
    {
        public ConsequentEventFeed(IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection,
                                   string key, [NotNull] DelayedEventFeed<TEvent, TOffset>[] feeds)
            : base(periodicJobRunnerWithLeaderElection, key, feeds)
        {
        }

        public override void RunFeed(TimeSpan delayBetweenIterations)
        {
            EventFeedsRegistry.Register(this);
            periodicJobRunnerWithLeaderElection.RunPeriodicJob(FormatJobName(Key), delayBetweenIterations, ExecuteFeeding, Initialize, Shutdown);
        }

        public override void StopFeed()
        {
            EventFeedsRegistry.Unregister(Key);
            periodicJobRunnerWithLeaderElection.StopPeriodicJob(FormatJobName(Key));
        }

        private void ExecuteFeeding()
        {
            Feeds.ForEach(x => x.ExecuteFeeding());
        }

        private void Initialize()
        {
            Feeds.ForEach(x => x.Initialize());
        }

        private void Shutdown()
        {
            Feeds.ForEach(x => x.Shutdown());
        }
    }
}