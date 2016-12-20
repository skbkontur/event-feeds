using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeeds.Implementations;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class BladeConfigurator<TOffset> : IBladeConfigurator<TOffset>, IBladeConfigurationContext
    {
        public BladeConfigurator([NotNull] string key)
        {
            this.key = key;
        }

        [NotNull]
        public IBladeConfigurator<TOffset> WithDelay(TimeSpan delay)
        {
            this.delay = delay;
            return this;
        }

        [NotNull]
        public IBladeConfigurator<TOffset> AndSendLagToGraphitePath([NotNull] Func<IBladeConfigurationContext, string> getGraphitePath)
        {
            this.getGraphitePath = getGraphitePath;
            return this;
        }

        [NotNull]
        public BladeConfigurator<TOffset> AndLeaderElectionBehavior(bool leaderElectionRequired)
        {
            this.leaderElectionRequired = leaderElectionRequired;
            return this;
        }

        public BladeConfigurator<TOffset> WithOffsetFactory(Func<IBladeConfigurationContext, IOffsetStorage<TOffset>> createOffsetStorage)
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
                key, globalTicksHolder, eventSource,
                (IOffsetStorage<long>)createOffsetStorage(this),
                consumer,
                graphiteClient,
                new EventFeedGraphitePaths(GetGraphiteActualizationLagPath()),
                delay,
                leaderElectionRequired);
        }

        [CanBeNull]
        private string GetGraphiteActualizationLagPath()
        {
            return getGraphitePath == null ? null : getGraphitePath(this);
        }

        [NotNull]
        public string BladeKey { get { return key; } }

        public TimeSpan Delay { get { return delay; } }

        private readonly string key;
        private TimeSpan delay;
        private Func<IBladeConfigurationContext, IOffsetStorage<TOffset>> createOffsetStorage;

        [CanBeNull]
        private Func<IBladeConfigurationContext, string> getGraphitePath;

        private bool leaderElectionRequired;
    }
}