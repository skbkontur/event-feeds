using System;

using JetBrains.Annotations;

using Vostok.Logging.Abstractions;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    internal class FallbackToIfEmptyOffsetStorage : IOffsetStorage<long?>
    {
        public FallbackToIfEmptyOffsetStorage([NotNull] IOffsetStorage<long?> innerStorage, long offsetTicks, [NotNull] ILog logger)
        {
            this.innerStorage = innerStorage;
            this.offsetTicks = offsetTicks;
            this.logger = logger.ForContext("DelayedEventFeed");
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
                logger.Info($"Set offset to {fallbackTime}");
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
        private readonly ILog logger;
        private readonly long offsetTicks;
    }
}