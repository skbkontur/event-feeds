using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IEventFeedsRunner
    {
        void ResetLocalState();
        void ExecuteForcedFeeding(TimeSpan delayUpperBound);
        bool AreEventsProcessedAt([NotNull] Timestamp timestamp);
        void Stop();
    }
}