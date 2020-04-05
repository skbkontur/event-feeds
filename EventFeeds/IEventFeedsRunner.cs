using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds.Building;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public interface IEventFeedsRunner
    {
        ( /*[NotNull]*/ BladeId BladeId, /*[CanBeNull]*/ Timestamp CurrentGlobalOffsetTimestamp)[] GetCurrentGlobalOffsetTimestamps();

        void ResetLocalState();

        void ExecuteForcedFeeding(TimeSpan delayUpperBound);

        bool AreEventsProcessedAt([NotNull] Timestamp timestamp);

        void Stop();
    }
}