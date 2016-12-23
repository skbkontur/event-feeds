using System;
using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IEventFeed
    {
        [NotNull]
        string Key { get; }

        bool LeaderElectionRequired { get; }

        void ExecuteFeeding();
        void ExecuteForcedFeeding();

        bool AreEventsProcessedAt(Timestamp timestamp);
        TimeSpan? GetCurrentActualizationLag();
        void Initialize();
        void Shutdown();
    }
}