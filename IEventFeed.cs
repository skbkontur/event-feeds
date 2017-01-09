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

        bool LeaderElectionRequired { get; }

        void ExecuteFeeding();
        void ExecuteForcedFeeding(TimeSpan delayUpperBound);

        bool AreEventsProcessedAt(Timestamp timestamp);
        TimeSpan? GetCurrentActualizationLag();
        void Initialize();
        void Shutdown();
    }
}