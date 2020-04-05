using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IGlobalTimeProvider
    {
        [NotNull]
        Timestamp GetNowTimestamp();
    }
}