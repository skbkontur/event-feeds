using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Catalogue.Core.EventFeed.Interfaces;
using SKBKontur.Catalogue.Core.LocalPersistentStoring;

namespace SKBKontur.Catalogue.Core.EventFeed.Providers.Implementation
{
    internal class FileSystemUnprocessedEventsStorage<T> : IUnprocessedEventsStorage<T>
    {
        public FileSystemUnprocessedEventsStorage(
            [NotNull] string path,
            [NotNull] Func<string, long, ILocalPersistentStorage<T[]>> createPersistentFileStorage)
        {
            effectivePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            const int mb = 1024 * 1024;
            localPersistentStorage = createPersistentFileStorage(effectivePath, 10 * mb);
            T[] events;
            if(localPersistentStorage.TryRead(out events))
            {
                foreach(var @event in events)
                    dict.TryAdd(@event, dummyValue);
            }
        }

        public void Dispose()
        {
            Flush();
            localPersistentStorage.Dispose();
        }

        public void RemoveEvent([NotNull] T elementaryEvent)
        {
            object dummy;
            dict.TryRemove(elementaryEvent, out dummy);
        }

        public T[] GetEvents()
        {
            return dict.Keys.ToArray();
        }

        public void Flush()
        {
            localPersistentStorage.Write(dict.Keys.ToArray());
        }

        public string GetDescription()
        {
            return string.Format("Unprocessed events storage in append-only files: Path: {0}", effectivePath);
        }

        public void AddEvents(IEnumerable<T> events)
        {
            events.ForEach(x => dict.TryAdd(x, dummyValue));
        }

        public void RemoveEvents(IEnumerable<T> oldEvents)
        {
            object dummy;
            oldEvents.ForEach(x => dict.TryRemove(x, out dummy));
        }

        private readonly string effectivePath;
        private readonly object dummyValue = new object();
        private readonly ILocalPersistentStorage<T[]> localPersistentStorage;
        private readonly ConcurrentDictionary<T, object> dict = new ConcurrentDictionary<T, object>();
    }
}