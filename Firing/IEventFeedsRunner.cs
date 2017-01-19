using System.Collections.Generic;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Firing
{
    public interface IEventFeedsRunner
    {
        [NotNull]
        List<IEventFeed> RunningFeeds { get; }

        void StopFeeds();
    }
}