using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface ICompositeEventFeed : IEventFeed
    {
        bool AreEventsProcessedAt([NotNull] Timestamp timestamp);

        void StopFeed();

        void RunFeed(TimeSpan delayBetweenIterations);
    }
}