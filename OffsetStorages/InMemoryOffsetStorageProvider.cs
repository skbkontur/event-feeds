using System.Collections.Concurrent;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    public class InMemoryOffsetStorageProvider
    {
        [NotNull]
        public IOffsetStorage<TOffset> OffsetStorage<TOffset>([NotNull] string key)
        {
            return (InMemoryOffsetStorage<TOffset>)offsetStorages.GetOrAdd(key, x => new InMemoryOffsetStorage<TOffset>());
        }

        private readonly ConcurrentDictionary<string, object> offsetStorages = new ConcurrentDictionary<string, object>();
    }
}