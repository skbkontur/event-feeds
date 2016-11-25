using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using MoreLinq;

using SKBKontur.Catalogue.Core.EventFeed.Interfaces;

namespace SKBKontur.Catalogue.Core.EventFeed.Providers.Implementation
{
    public class InMemoryUnprocessedEventsStorage<T> : IUnprocessedEventsStorage<T>
    {
        public string GetDescription()
        {
            return string.Format("In memory unprocessed events storage");
        }

        public void AddEvents(IEnumerable<T> events)
        {
            events.ForEach(x => elementaryEvents.TryAdd(x, new object()));
        }

        public void RemoveEvents(IEnumerable<T> events)
        {
            object dummy;
            events.ForEach(x => elementaryEvents.TryRemove(x, out dummy));
        }

        public T[] GetEvents()
        {
            return elementaryEvents.Keys.ToArray();
        }

        public void Flush()
        {
        }

        private readonly ConcurrentDictionary<T, object> elementaryEvents = new ConcurrentDictionary<T, object>();
    }
}