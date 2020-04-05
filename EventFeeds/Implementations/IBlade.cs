using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds.Building;

namespace SkbKontur.EventFeeds.Implementations
{
    public interface IBlade
    {
        [NotNull]
        BladeId BladeId { get; }

        void Initialize();
        void Shutdown();
        void ResetLocalState();
        void ExecuteFeeding();
        void ExecuteForcedFeeding(TimeSpan delayUpperBound);

        [CanBeNull]
        Timestamp GetCurrentGlobalOffsetTimestamp();

        bool AreEventsProcessedAt([NotNull] Timestamp timestamp);
    }
}