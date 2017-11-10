using System;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IEventFeed
    {
        [NotNull]
        string Key { get; }

        TimeSpan Delay { get; }

        void ExecuteForcedFeeding(TimeSpan delayUpperBound);
    }
}