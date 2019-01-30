using JetBrains.Annotations;

using Vostok.Logging.Abstractions;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    public static class OffsetStorageExtensions
    {
        [NotNull]
        public static IOffsetStorage<long?> AndRollbackIfOffsetEmpty([NotNull] this IOffsetStorage<long?> offsetStorage, long rollback, ILog logger)
        {
            return new RollbackOnEmptyOffsetStorage(offsetStorage, rollback, logger);
        }

        [NotNull]
        public static IOffsetStorage<long?> AndFallbackToIfOffsetEmpty([NotNull] this IOffsetStorage<long?> offsetStorage, long offsetValue, ILog logger)
        {
            return new FallbackToIfEmptyOffsetStorage(offsetStorage, offsetValue, logger);
        }
    }
}