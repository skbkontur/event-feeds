using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IEventFeed
    {
        [NotNull]
        string Key { get; }

        TimeSpan Delay { get; }

        void ResetLocalOffset();

        void ExecuteFeeding();
        void ExecuteForcedFeeding(TimeSpan delayUpperBound);

        bool AreEventsProcessedAt([NotNull] Timestamp timestamp);
    }
}