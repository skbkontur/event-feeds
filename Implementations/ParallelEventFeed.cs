using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class ParallelEventFeed<TEvent, TOffset> : CompositeEventFeedBase<TEvent, TOffset>
    {
        public ParallelEventFeed([NotNull] IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection,
                                 [NotNull] string key, [NotNull] DelayedEventFeed<TEvent, TOffset>[] feeds)
            : base(periodicJobRunnerWithLeaderElection, key, feeds)
        {
        }

        public override void RunFeed(TimeSpan delayBetweenIterations)
        {
            foreach(var feed in Feeds)
            {
                EventFeedsRegistry.Register(feed);
                periodicJobRunnerWithLeaderElection.RunPeriodicJob(FormatJobName(feed.Key), delayBetweenIterations, feed.ExecuteFeeding, feed.Initialize, feed.Shutdown);
            }
        }

        public override void StopFeed()
        {
            foreach(var feed in Feeds)
            {
                periodicJobRunnerWithLeaderElection.StopPeriodicJob(FormatJobName(feed.Key));
                EventFeedsRegistry.Unregister(feed.Key);
            }
        }
    }
}