using System;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class CompositeEventFeed : IEventFeed
    {
        public CompositeEventFeed([NotNull] string feedKey, [NotNull, ItemNotNull] IEventFeed[] feeds)
        {
            FeedKey = feedKey;
            this.feeds = feeds;
        }

        [NotNull]
        public string FeedKey { get; }

        public void Initialize()
        {
            feeds.ForEach(feed => feed.Initialize());
        }

        public void Shutdown()
        {
            feeds.ForEach(feed => feed.Shutdown());
        }

        public void ExecuteFeeding()
        {
            feeds.ForEach(feed => feed.ExecuteFeeding());
        }

        public void ResetLocalState()
        {
            feeds.ForEach(feed => feed.ResetLocalState());
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            feeds.ForEach(feed => feed.ExecuteForcedFeeding(delayUpperBound));
        }

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            return feeds.All(feed => feed.AreEventsProcessedAt(timestamp));
        }

        private readonly IEventFeed[] feeds;
    }
}