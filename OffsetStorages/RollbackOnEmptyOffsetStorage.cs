using System;
using JetBrains.Annotations;
using log4net;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    internal class RollbackOnEmptyOffsetStorage : IOffsetStorage<long?>
    {
        public RollbackOnEmptyOffsetStorage([NotNull] IOffsetStorage<long?> innerStorage, long rollbackTicks)
        {
            this.innerStorage = innerStorage;
            this.rollbackTicks = rollbackTicks;
        }

        public string GetDescription()
        {
            return string.Format("{0} and rollback if empty for {1}", innerStorage.GetDescription(), TimeSpan.FromTicks(rollbackTicks));
        }

        public long? Read()
        {
            var result = innerStorage.Read();
            if(!result.HasValue || result <= 0)
            {
                var rolledBackTime = DateTime.UtcNow.AddTicks(-rollbackTicks);
                logger.InfoFormat("Rolled back offset to {0}", rolledBackTime);
                Write(rolledBackTime.Ticks);
                result = innerStorage.Read();
            }
            return result;
        }

        public void Write(long? newOffset)
        {
            innerStorage.Write(newOffset);
        }

        private readonly IOffsetStorage<long?> innerStorage;
        private readonly long rollbackTicks;
        private static readonly ILog logger = LogManager.GetLogger(typeof(RollbackOnEmptyOffsetStorage));
    }
}