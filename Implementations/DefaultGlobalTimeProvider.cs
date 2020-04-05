using JetBrains.Annotations;

using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class DefaultGlobalTimeProvider : IGlobalTimeProvider
    {
        public DefaultGlobalTimeProvider(IGlobalTime globalTime)
        {
            this.globalTime = globalTime;
        }

        [NotNull]
        public Timestamp GetNowTimestamp()
        {
            return globalTime.UpdateNowTimestamp();
        }

        private readonly IGlobalTime globalTime;
    }
}