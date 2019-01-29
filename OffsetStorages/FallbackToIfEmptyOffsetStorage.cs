using System;

using JetBrains.Annotations;

using log4net;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    internal class FallbackToIfEmptyOffsetStorage : IOffsetStorage<long?>
    {
        public FallbackToIfEmptyOffsetStorage([NotNull] IOffsetStorage<long?> innerStorage, long offsetTicks)
        {
            this.innerStorage = innerStorage;
            this.offsetTicks = offsetTicks;
        }

        public string GetDescription()
        {
            return $"{innerStorage.GetDescription()} and set value to {new DateTime(offsetTicks, DateTimeKind.Utc)} if empty";
        }

        public long? Read()
        {
            var result = innerStorage.Read();
            if (!result.HasValue || result <= 0)
            {
                var fallbackTime = new DateTime(offsetTicks, DateTimeKind.Utc);
                logger.InfoFormat("Set offset to {0}", fallbackTime);
                Write(offsetTicks);
                result = innerStorage.Read();
            }
            return result;
        }

        public void Write(long? newOffset)
        {
            innerStorage.Write(newOffset);
        }

        private readonly IOffsetStorage<long?> innerStorage;
        private static readonly ILog logger = LogManager.GetLogger(typeof(RollbackOnEmptyOffsetStorage));
        private readonly long offsetTicks;
    }
}