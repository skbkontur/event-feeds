using System;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeed.Interfaces
{
    public interface IEventFeed
    {
        [NotNull]
        string Key { get; }

        bool LeaderElectionRequired { get; }

        void ExecuteFeeding();
        void ExecuteForcedFeeding();

        bool AreEventsProcessedAt(DateTime dateTime);
        TimeSpan? GetCurrentActualizationLag();
        void Initialize();
        void Shutdown();
    }
}