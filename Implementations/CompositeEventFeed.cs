using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class CompositeEventFeed<TEvent, TOffset> : IEventFeed
    {
        public CompositeEventFeed([NotNull] string key, [NotNull] List<DelayedEventFeed<TEvent, TOffset>> feeds)
        {
            this.feeds = feeds;
            Key = key;
        }

        public string Key { get; private set; }

        public TimeSpan Delay { get { return feeds.Min(feed => feed.Delay); } }

        public void ResetLocalOffset()
        {
            feeds.ForEach(feed => feed.ResetLocalOffset());
        }

        public void ExecuteFeeding()
        {
            feeds.ForEach(feed => feed.ExecuteFeeding());
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            feeds.Where(feed => feed.Delay <= delayUpperBound).ForEach(feed => feed.ExecuteForcedFeeding(delayUpperBound));
        }

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            return feeds.All(feed => feed.AreEventsProcessedAt(timestamp));
        }

        private readonly List<DelayedEventFeed<TEvent, TOffset>> feeds;
    }
}