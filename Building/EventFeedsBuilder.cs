using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MoreLinq;
using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeeds.Firing;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class EventFeedsBuilder<TEvent, TOffset> : IEventFeedsBuilder<TEvent, TOffset> where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
    {
        public EventFeedsBuilder(
            [NotNull] string key,
            [NotNull] IGlobalTicksHolder globalTicksHolder,
            [NotNull] ICatalogueGraphiteClient graphiteClient,
            [NotNull] Func<string, List<IEventFeed>, IEventFeedsFireRaiser> createEventFeeds)
        {
            this.key = key;
            this.globalTicksHolder = globalTicksHolder;
            this.graphiteClient = graphiteClient;
            this.createEventFeeds = createEventFeeds;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> WithEventSource([NotNull] IEventSource<TEvent> eventSource)
        {
            this.eventSource = eventSource;
            return this;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> WithConsumer([NotNull] IEventConsumer<TEvent> eventConsumer)
        {
            this.consumer = eventConsumer;
            return this;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> WithOffsetStorageFactory([NotNull] Func<IBladeConfigurationContext, IOffsetStorage<TOffset>> createOffsetStorage)
        {
            this.offsetStorageFactory = createOffsetStorage;
            return this;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> WithBlade([NotNull] string bladeKey, [NotNull] Action<IBladeConfigurator<TOffset>> bladeConfigurator)
        {
            var configurator = new BladeConfigurator<TOffset>(bladeKey);
            bladeConfigurator(configurator);
            blades.Add(configurator);
            return this;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> AndUnprocessedEvents([NotNull] IUnprocessedEventsStorage<TEvent> unprocessedEventsStorage)
        {
            this.unprocessedEventsStorage = unprocessedEventsStorage;
            unprocessedBladeConfigurator = new UnprocessedEventsBladeConfigurator<TEvent>(key + "_UnprocessedEvents");
            return this;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> AndLeaderElectionRequired()
        {
            this.leaderElectionRequired = true;
            return this;
        }

        [NotNull]
        private IEventFeedsFireRaiser Create()
        {
            var eventFeedBlades = blades
                .Pipe(blade => blade.WithOffsetFactory(offsetStorageFactory).AndLeaderElectionBehavior(leaderElectionRequired))
                .Select(c => c.Create(globalTicksHolder, eventSource, consumer, graphiteClient, unprocessedEventsStorage))
                .ToList();
            if(unprocessedBladeConfigurator != null)
            {
                if (unprocessedEventsStorage == null)
                    throw new InvalidProgramStateException("UnprocessedBladeConfigurator exists, but unprocessedEventsStorage is null");
                unprocessedBladeConfigurator.WithUnprocessedEventsStorage(unprocessedEventsStorage).AndLeaderElectionBehavior(leaderElectionRequired);
                eventFeedBlades.Add(unprocessedBladeConfigurator.Create(consumer, globalTicksHolder));
            }
            return createEventFeeds(key, eventFeedBlades);
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> NoParallel()
        {
            this.noParallel = true;
            return this;
        }

        [NotNull]
        public IEventFeedsFireRaiser FirePeriodicTasks()
        {
            var fireRaiser = Create();
            if(noParallel)
                fireRaiser = fireRaiser.NoParallel();
            fireRaiser.FirePeriodicTasks();
            return fireRaiser;
        }

        private readonly string key;
        private readonly IGlobalTicksHolder globalTicksHolder;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly Func<string, List<IEventFeed>, IEventFeedsFireRaiser> createEventFeeds;
        private IEventSource<TEvent> eventSource;
        private IEventConsumer<TEvent> consumer;
        private Func<IBladeConfigurationContext, IOffsetStorage<TOffset>> offsetStorageFactory;
        private readonly List<BladeConfigurator<TOffset>> blades = new List<BladeConfigurator<TOffset>>();
        private IUnprocessedEventsStorage<TEvent> unprocessedEventsStorage;
        private UnprocessedEventsBladeConfigurator<TEvent> unprocessedBladeConfigurator;
        private bool leaderElectionRequired;
        private bool noParallel;
    }
}