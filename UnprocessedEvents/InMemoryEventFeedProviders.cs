using System.Collections.Concurrent;
using JetBrains.Annotations;
using SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages;

namespace SKBKontur.Catalogue.Core.EventFeeds.UnprocessedEvents
{
    public class InMemoryEventFeedProviders
    {
        [NotNull]
        public IOffsetStorage<TOffset> OffsetStorage<TOffset>([NotNull] string key)
        {
            return (InMemoryOffsetStorage<TOffset>)genericLastEventStorages.GetOrAdd(key, x => new InMemoryOffsetStorage<TOffset>());
        }

        [NotNull]
        public IUnprocessedEventsStorage<TEvent> UnprocessedEventStorageImpl<TEvent>([NotNull] string key)
        {
            return (IUnprocessedEventsStorage<TEvent>)unprocessedEventsStorages.GetOrAdd(key, x => new InMemoryUnprocessedEventsStorage<TEvent>());
        }

        private readonly ConcurrentDictionary<string, object> genericLastEventStorages = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> unprocessedEventsStorages = new ConcurrentDictionary<string, object>();
    }
}