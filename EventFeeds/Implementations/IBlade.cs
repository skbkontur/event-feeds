using System;
using System.Threading;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.EventFeeds.Implementations
{
    public interface IBlade
    {
        [NotNull]
        BladeId BladeId { get; }

        void Initialize();
        void Shutdown();
        void ResetLocalState();
        void ExecuteFeeding(CancellationToken leaderLockExpirationToken);
        void ExecuteForcedFeeding(TimeSpan delayUpperBound);

        [CanBeNull]
        Timestamp GetCurrentGlobalOffsetTimestamp();

        bool AreEventsProcessedAt([NotNull] Timestamp timestamp);
    }
}