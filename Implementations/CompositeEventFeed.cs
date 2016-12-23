using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    internal class CompositeEventFeed : IEventFeed
    {
        public CompositeEventFeed([NotNull] string key, [NotNull] IEnumerable<IEventFeed> feeds)
        {
            this.feeds = feeds.ToList();
            Key = key;
            ValidateLeaderElectionRequirementsAreSame();
        }

        private void ValidateLeaderElectionRequirementsAreSame()
        {
            for(var i = 0; i < feeds.Count - 1; i++)
            {
                if(feeds[i].LeaderElectionRequired != feeds[i + 1].LeaderElectionRequired)
                {
                    throw new InvalidProgramStateException(string.Format(
                        "It is impossible to create CompositeEventFeed(Key = {0})) with different leader election requirements. " +
                        "But its different for feeds with keys '{1}'({2}), '{3}'({4})",
                        Key, feeds[i].Key, feeds[i].LeaderElectionRequired, feeds[i + 1].Key, feeds[i + 1].LeaderElectionRequired));
                }
            }
        }

        public string Key { get; private set; }

        public bool LeaderElectionRequired { get { return feeds.First().LeaderElectionRequired; } }

        public void ExecuteFeeding()
        {
            feeds.ForEach(feed => feed.ExecuteFeeding());
        }

        public void ExecuteForcedFeeding()
        {
            feeds.ForEach(feed => feed.ExecuteForcedFeeding());
        }

        public bool AreEventsProcessedAt(Timestamp timestamp)
        {
            return feeds.All(feed => feed.AreEventsProcessedAt(timestamp));
        }

        public TimeSpan? GetCurrentActualizationLag()
        {
            TimeSpan? result = null;
            foreach(var feed in feeds)
            {
                var lag = feed.GetCurrentActualizationLag();
                if(lag == null)
                    continue;
                if(result == null)
                    result = lag;
                result = result.Value > lag.Value ? lag : result;
            }
            return result;
        }

        public void Initialize()
        {
            feeds.ForEach(feed => feed.Initialize());
        }

        public void Shutdown()
        {
            feeds.ForEach(feed => feed.Shutdown());
        }

        private readonly List<IEventFeed> feeds;
    }
}