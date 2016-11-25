using System.Collections.Generic;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeed.Interfaces
{
    public interface IUnprocessedEventsStorage<T>
    {
        [NotNull]
        string GetDescription();

        void AddEvents([NotNull] IEnumerable<T> events);
        void RemoveEvents([NotNull] IEnumerable<T> events);

        [NotNull]
        T[] GetEvents();

        void Flush();
    }
}