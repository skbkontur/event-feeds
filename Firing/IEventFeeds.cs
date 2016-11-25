using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeed.Interfaces;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl
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