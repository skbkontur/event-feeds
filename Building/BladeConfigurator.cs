using System;
using System.Collections.Generic;
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
        public IBladeConfigurator<TOffset> AndLeaderElectionRequired()
        {
            this.leaderElectionRequired = true;
            return this;
        }

        public void WithOffsetFactory(Func<IBladeConfigurationContext, IOffsetStorage<TOffset>> createOffsetStorage)
        {
            this.createOffsetStorage = createOffsetStorage;
        }

        public IEventFeed Create<TEvent>(
            [NotNull] IGlobalTicksHolder globalTicksHolder,
            [NotNull] IEventLogEventSource<TEvent> eventSource,
            [NotNull] IEventConsumer<TEvent> consumer,
            [NotNull] ICatalogueGraphiteClient graphiteClient,
            [CanBeNull] IUnprocessedEventsStorage<TEvent> unprocessedEventsStorage) where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            return new SingleRazorEventFeedWithDelayImpl<TEvent>(
                key, globalTicksHolder, eventSource,
                (IOffsetStorage<long>)createOffsetStorage(this),
                consumer,
                unprocessedEventsStorage ?? new ThrowAwayUnprocessedEventStorage<TEvent>(),
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

        private class ThrowAwayUnprocessedEventStorage<T> : IUnprocessedEventsStorage<T>
        {
            public string GetDescription()
            {
                return "Throw all unprocessed event away";
            }

            public void AddEvents(IEnumerable<T> events)
            {
            }

            public void RemoveEvents(IEnumerable<T> events)
            {
            }

            public T[] GetEvents()
            {
                return emptyArray;
            }

            public void Flush()
            {
            }

            private readonly T[] emptyArray = new T[0];
        }
    }
}