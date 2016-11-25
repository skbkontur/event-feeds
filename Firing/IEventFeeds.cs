using System.Collections.Generic;
using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Firing
{
    public interface IEventFeeds
    {
        [NotNull]
        string Key { get; }

        [NotNull]
        IEnumerable<IEventFeed> Feeds();

        [NotNull]
        IEventFeeds AsOneFeed();

        void RegisterPeriodicTasks();

        void UnregisterPeriodicTasks();
    }
}