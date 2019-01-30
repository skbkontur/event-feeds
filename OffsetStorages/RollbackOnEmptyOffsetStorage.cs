using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

using Vostok.Logging.Abstractions;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    internal class RollbackOnEmptyOffsetStorage : IOffsetStorage<long?>
    {
        public RollbackOnEmptyOffsetStorage([NotNull] IOffsetStorage<long?> innerStorage, long rollbackTicks, [NotNull] ILog logger)
        {
            this.innerStorage = innerStorage;
            this.rollbackTicks = rollbackTicks;
            this.logger = logger.ForContext("DelayedEventFeed");
        }

        public string GetDescription()
        {
            return $"{innerStorage.GetDescription()} and rollback if empty for {TimeSpan.FromTicks(rollbackTicks)}";
        }

        public long? Read()
        {
            var result = innerStorage.Read();
            if (!result.HasValue || result <= 0)
            {
                var rolledBackTime = Timestamp.Now.AddTicks(-rollbackTicks);
                logger.Info($"Rolled back offset to {rolledBackTime}");
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
        private readonly ILog logger;
    }
}