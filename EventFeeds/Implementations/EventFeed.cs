using System;
using System.Linq;
using System.Threading;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.EventFeeds.Implementations
{
    public class EventFeed : IRunningEventFeed
    {
        public EventFeed([NotNull] string feedKey, [NotNull, ItemNotNull] IBlade[] blades)
        {
            FeedKey = feedKey;
            this.blades = blades;
        }

        [NotNull]
        public string FeedKey { get; }

        public void Initialize()
        {
            foreach (var blade in blades)
                blade.Initialize();
        }

        public void Shutdown()
        {
            foreach (var blade in blades)
                blade.Shutdown();
        }

        public void ExecuteFeeding(CancellationToken leaderLockExpirationToken)
        {
            foreach (var blade in blades)
                blade.ExecuteFeeding(leaderLockExpirationToken);
        }

        public void ResetLocalState()
        {
            foreach (var blade in blades)
                blade.ResetLocalState();
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            foreach (var blade in blades)
                blade.ExecuteForcedFeeding(delayUpperBound);
        }

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            return blades.All(blade => blade.AreEventsProcessedAt(timestamp));
        }

        public ( /*[NotNull]*/ BladeId BladeId, /*[CanBeNull]*/ Timestamp CurrentGlobalOffsetTimestamp)[] GetCurrentGlobalOffsetTimestamps()
        {
            return blades.Select(x => (x.BladeId, CurrentGlobalOffsetTimestamp : x.GetCurrentGlobalOffsetTimestamp())).ToArray();
        }

        private readonly IBlade[] blades;
    }
}