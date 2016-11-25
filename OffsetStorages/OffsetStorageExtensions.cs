using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeed.Interfaces;
using SKBKontur.Catalogue.Core.EventFeed.Providers.Implementation;

namespace SKBKontur.Catalogue.Core.EventFeed.Providers
{
    public static class OffsetStorageExtensions
    {
        [NotNull]
        public static IOffsetStorage<long> AndRollbackIfOffsetEmpty([NotNull] this IOffsetStorage<long> offsetStorage, long rollback)
        {
            return new RollbackOnEmptyOffsetStorage(offsetStorage, rollback);
        }
        
        [NotNull]
        public static IOffsetStorage<long> AndFallbackToIfOffsetEmpty([NotNull] this IOffsetStorage<long> offsetStorage, long offsetValue)
        {
            return new FallbackToIfEmptyOffsetStorage(offsetStorage, offsetValue);
        }
    }
}