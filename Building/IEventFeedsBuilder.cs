using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.Firing;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public interface IEventFeedsBuilder<TEvent, TOffset>
    {
        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> WithEventSource([NotNull] IEventSource<TEvent, TOffset> eventSource);

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> WithConsumer([NotNull] IEventConsumer<TEvent, TOffset> eventConsumer);

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> WithOffsetStorageFactory([NotNull] Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage);

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> WithOffsetInterpreter([NotNull] IOffsetInterpreter<TOffset> offsetInterpreter);

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> WithBlade([NotNull] string bladeKey, TimeSpan delay);

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> InParallel();

        [NotNull]
        IEventFeedsRunner RunFeeds(TimeSpan actualizeInterval);
    }
}