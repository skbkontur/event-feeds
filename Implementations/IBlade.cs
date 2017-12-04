using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.Building;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
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