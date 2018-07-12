using JetBrains.Annotations;

using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class DefaultGlobalTimeProvider : IGlobalTimeProvider
    {
        public DefaultGlobalTimeProvider(IGlobalTicksHolder globalTicksHolder)
        {
            this.globalTicksHolder = globalTicksHolder;
        }

        [NotNull]
        public Timestamp GetNowTimestamp()
        {
            return new Timestamp(globalTicksHolder.GetNowTicks());
        }

        private readonly IGlobalTicksHolder globalTicksHolder;
    }
}