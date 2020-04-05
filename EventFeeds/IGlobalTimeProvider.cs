using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public interface IGlobalTimeProvider
    {
        [NotNull]
        Timestamp GetNowTimestamp();
    }
}