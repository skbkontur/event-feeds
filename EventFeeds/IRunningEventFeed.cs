using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public interface IRunningEventFeed
    {
        [NotNull]
        string FeedKey { get; }

        ( /*[NotNull]*/ BladeId BladeId, /*[CanBeNull]*/ Timestamp CurrentGlobalOffsetTimestamp)[] GetCurrentGlobalOffsetTimestamps();
    }
}