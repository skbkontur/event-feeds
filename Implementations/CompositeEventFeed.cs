using System;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class CompositeEventFeed : IEventFeed
    {
        public CompositeEventFeed([NotNull] string feedKey, [NotNull, ItemNotNull] IBlade[] blades)
        {
            FeedKey = feedKey;
            this.blades = blades;
        }

        [NotNull]
        public string FeedKey { get; }

        public void Initialize()
        {
            blades.ForEach(feed => feed.Initialize());
        }

        public void Shutdown()
        {
            blades.ForEach(feed => feed.Shutdown());
        }

        public void ExecuteFeeding()
        {
            blades.ForEach(feed => feed.ExecuteFeeding());
        }

        public void ResetLocalState()
        {
            blades.ForEach(feed => feed.ResetLocalState());
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            blades.ForEach(feed => feed.ExecuteForcedFeeding(delayUpperBound));
        }

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            return blades.All(feed => feed.AreEventsProcessedAt(timestamp));
        }

        private readonly IBlade[] blades;
    }
}