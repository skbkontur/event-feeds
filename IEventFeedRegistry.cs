using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeed.Interfaces;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl
{
    public interface IEventFeedRegistry
    {
        void Register([NotNull] IEventFeed feed);

        [NotNull]
        IEnumerable<IEventFeed> GetAll();

        [NotNull]
        IEventFeed GetByKey([NotNull] string key);
    }

    public class EventFeedRegistry : IEventFeedRegistry
    {
        public void Register([NotNull] IEventFeed feed)
        {
            if(feeds.ContainsKey(feed.Key))
                throw new InvalidProgramStateException(string.Format("Feed with key {0} has already been added into registry", feed.Key));
            feeds.Add(feed.Key, feed);
        }

        [NotNull]
        public IEnumerable<IEventFeed> GetAll()
        {
            return feeds.Values;
        }

        [NotNull]
        public IEventFeed GetByKey([NotNull] string key)
        {
            if(!feeds.ContainsKey(key))
                throw new InvalidProgramStateException(string.Format("Feed with key {0} is not registered", key));
            return feeds[key];
        }

        private readonly Dictionary<string, IEventFeed> feeds = new Dictionary<string, IEventFeed>();
    }
}