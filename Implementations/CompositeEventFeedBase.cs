using System;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public abstract class CompositeEventFeedBase<TEvent, TOffset> : ICompositeEventFeed
    {
        protected CompositeEventFeedBase([NotNull] IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection,
                                         [NotNull] string key, [NotNull] DelayedEventFeed<TEvent, TOffset>[] feeds)
        {
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            Feeds = feeds;
            Key = key;
        }

        [NotNull]
        public string Key { get; }

        public TimeSpan Delay { get { return Feeds.Min(feed => feed.Delay); } }

        public abstract void RunFeed(TimeSpan delayBetweenIterations);

        public abstract void StopFeed();

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            Feeds.Where(feed => feed.Delay <= delayUpperBound).ForEach(feed => feed.ExecuteForcedFeeding(delayUpperBound));
        }

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            return Feeds.All(feed => feed.AreEventsProcessedAt(timestamp));
        }

        [NotNull]
        protected static string FormatJobName([NotNull] string key)
        {
            return $"{key}-PeriodicJob";
        }

        [NotNull]
        protected DelayedEventFeed<TEvent, TOffset>[] Feeds { get; }

        [NotNull]
        protected readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
    }
}