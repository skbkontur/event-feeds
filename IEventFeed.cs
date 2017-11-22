using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IEventFeed
    {
        [NotNull]
        string FeedKey { get; }

        void ResetLocalState();

        bool AreEventsProcessedAt([NotNull] Timestamp timestamp);

        void ExecuteForcedFeeding(TimeSpan delayUpperBound);
    }
}