using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeeds.Implementations;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class BladeConfigurator<TOffset>
    {
        public BladeConfigurator([NotNull] string key, TimeSpan delay)
        {
            bladeId = new BladeId(key, delay);
        }

        [NotNull]
        public BladeConfigurator<TOffset> AndLeaderElectionBehavior(bool leaderElectionRequired)
        {
            this.leaderElectionRequired = leaderElectionRequired;
            return this;
        }

        public BladeConfigurator<TOffset> WithOffsetFactory(Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage)
        {
            this.createOffsetStorage = createOffsetStorage;
            return this;
        }

        public IEventFeed Create<TEvent>(
            [NotNull] IGlobalTicksHolder globalTicksHolder,
            [NotNull] IEventSource<TEvent> eventSource,
            [NotNull] IEventConsumer<TEvent> consumer,
            [NotNull] ICatalogueGraphiteClient graphiteClient) where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            return new DelayedEventFeed<TEvent>(
                globalTicksHolder, eventSource,
                (IOffsetStorage<long>)createOffsetStorage(bladeId),
                consumer,
                graphiteClient,
                bladeId,
                leaderElectionRequired);
        }

        
        private Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage;
        private bool leaderElectionRequired;
        private readonly BladeId bladeId;
    }
}