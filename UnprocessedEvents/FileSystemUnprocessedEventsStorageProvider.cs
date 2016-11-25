using GroboContainer.Core;
using JetBrains.Annotations;
using SKBKontur.Catalogue.Core.LocalPersistentStoring;

namespace SKBKontur.Catalogue.Core.EventFeeds.UnprocessedEvents
{
    public class FileSystemUnprocessedEventsStorageProvider
    {
        public FileSystemUnprocessedEventsStorageProvider(IContainer container)
        {
            this.container = container;
        }

        [NotNull]
        public IUnprocessedEventsStorage<T> CreateUnprocessedEventStorage<T>([NotNull] string path)
        {
            return new FileSystemUnprocessedEventsStorage<T>(path, (p, s) => container.Create<string, long, ILocalPersistentStorage<T[]>>(p, s));
        }

        private readonly IContainer container;
    }
}