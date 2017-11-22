using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    // todo (andrew, 22.11.2017): get rid of this dirty static hack (EventFeedsRegistry)
    public static class EventFeedsRegistry
    {
        public static void Register([NotNull] IEventFeed feed)
        {
            if(feeds.ContainsKey(feed.FeedKey))
                throw new InvalidProgramStateException($"Feed with feedKey {feed.FeedKey} has already been added into registry");
            feeds.Add(feed.FeedKey, feed);
        }

        public static void Unregister([NotNull] string feedKey)
        {
            if(!feeds.ContainsKey(feedKey))
                throw new InvalidProgramStateException($"Feed with feedKey {feedKey} is not registered");
            feeds.Remove(feedKey);
        }

        [NotNull]
        public static IEnumerable<IEventFeed> GetAll()
        {
            return feeds.Values;
        }

        private static readonly Dictionary<string, IEventFeed> feeds = new Dictionary<string, IEventFeed>();
    }
}