using System;
using JetBrains.Annotations;
using log4net;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    internal class FallbackToIfEmptyOffsetStorage : IOffsetStorage<long?>
    {
        public FallbackToIfEmptyOffsetStorage(
            [NotNull] IOffsetStorage<long?> innerStorage,
            long offsetTicks
            )
        {
            this.innerStorage = innerStorage;
            this.offsetTicks = offsetTicks;
        }

        public string GetDescription()
        {
            return string.Format("{0} and set value to {1} if empty", innerStorage.GetDescription(), new DateTime(offsetTicks, DateTimeKind.Utc));
        }

        public long? Read(string key)
        {
            var result = innerStorage.Read(key);
            if(!result.HasValue || result <= 0)
            {
                var fallbackTime = new DateTime(offsetTicks, DateTimeKind.Utc);
                logger.InfoFormat("Set offset to {0}", fallbackTime);
                Write(key, offsetTicks);
                result = innerStorage.Read(key);
            }
            return result;
        }

        public void Write(string key, long? offset)
        {
            innerStorage.Write(key, offset);
        }

        private readonly IOffsetStorage<long?> innerStorage;
        private static readonly ILog logger = LogManager.GetLogger(typeof(RollbackOnEmptyOffsetStorage));
        private readonly long offsetTicks;
    }
}