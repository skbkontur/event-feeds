using System.Collections.Generic;
using JetBrains.Annotations;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public static class EventFeedsRegistry
    {
        public static void Register([NotNull] IEventFeed feed)
        {
            if(feeds.ContainsKey(feed.Key))
                throw new InvalidProgramStateException(string.Format("Feed with key {0} has already been added into registry", feed.Key));
            feeds.Add(feed.Key, feed);
        }

        public static void Unregister([NotNull] string key)
        {
            if (!feeds.ContainsKey(key))
                throw new InvalidProgramStateException(string.Format("Feed with key {0} is not registered", key));
            feeds.Remove(key);
        }

        [NotNull]
        public static IEnumerable<IEventFeed> GetAll()
        {
            return feeds.Values;
        }

        [NotNull]
        public static IEventFeed GetByKey([NotNull] string key)
        {
            if(!feeds.ContainsKey(key))
                throw new InvalidProgramStateException(string.Format("Feed with key {0} is not registered", key));
            return feeds[key];
        }

        private static readonly Dictionary<string, IEventFeed> feeds = new Dictionary<string, IEventFeed>();
    }
}