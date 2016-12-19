using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeeds.Firing;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public interface IEventFeedsBuilder<TEvent, TOffset> where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
    {
        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> WithEventSource([NotNull] IEventSource<TEvent> eventSource);

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> WithConsumer([NotNull] IEventConsumer<TEvent> eventConsumer);

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> WithOffsetStorageFactory([NotNull] Func<IBladeConfigurationContext, IOffsetStorage<TOffset>> createOffsetStorage);

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> WithBlade([NotNull] string bladeKey, [NotNull] Action<IBladeConfigurator<TOffset>> bladeConfigurator);

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> AndUnprocessedEvents([NotNull] IUnprocessedEventsStorage<TEvent> unprocessedEventsStorage);

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> AndLeaderElectionRequired();

        [NotNull]
        IEventFeedsBuilder<TEvent, TOffset> NoParallel();

        [NotNull]
        IEventFeedsFireRaiser FirePeriodicTasks();
    }
}