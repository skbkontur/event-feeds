using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.EventFeeds.Firing;
using SKBKontur.Catalogue.Core.EventFeeds.Implementations;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class EventFeedsBuilder<TEvent, TOffset> : IEventFeedsBuilder<TEvent, TOffset>
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
        public IEventFeedsBuilder<TEvent, TOffset> WithEventSource([NotNull] IEventSource<TEvent, TOffset> eventSource)
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
        public IEventFeedsBuilder<TEvent, TOffset> WithOffsetStorageFactory([NotNull] Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage)
        {
            this.offsetStorageFactory = createOffsetStorage;
            return this;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> WithOffsetInterpreter([NotNull] IOffsetInterpreter<TOffset> offsetInterpreter)
        {
            this.offsetInterpreter = offsetInterpreter;
            return this;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> WithBlade([NotNull] string bladeKey, TimeSpan delay)
        {
            blades.Add(new BladeConfigurator<TOffset>(bladeKey, delay));
            return this;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> WithLeaderElection()
        {
            this.leaderElectionRequired = true;
            return this;
        }

        [NotNull]
        private IEventFeedsFireRaiser Create()
        {
            var eventFeedBlades = blades
                .Pipe(blade => blade
                                   .WithOffsetFactory(offsetStorageFactory)
                                   .WithOffsetInterpreter(GetOffsetInterpreter())
                                   .AndLeaderElectionBehavior(leaderElectionRequired))
                .Select(c => c.Create(globalTicksHolder, eventSource, consumer, graphiteClient))
                .ToList();
            return createEventFeeds(key, eventFeedBlades);
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> InParallel()
        {
            this.inParallel = true;
            return this;
        }

        [NotNull]
        public IEventFeedsFireRaiser FirePeriodicTasks(TimeSpan actualizeInterval)
        {
            var fireRaiser = Create();
            if(!inParallel)
                fireRaiser = fireRaiser.NoParallel();
            fireRaiser.FirePeriodicTasks(actualizeInterval);
            return fireRaiser;
        }

        [NotNull]
        private IOffsetInterpreter<TOffset> GetOffsetInterpreter()
        {
            if(offsetInterpreter != null)
                return offsetInterpreter;
            if(typeof(TOffset) == typeof(long))
                return (IOffsetInterpreter<TOffset>)StandardTicksOffsetInterpreter.Instance;
            throw new InvalidProgramStateException(string.Format("OffsetInterpreter has not set, but for type {0} there is no default interpreter", typeof(TOffset).FullName));
        }

        private readonly string key;
        private readonly IGlobalTicksHolder globalTicksHolder;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly Func<string, List<IEventFeed>, IEventFeedsFireRaiser> createEventFeeds;
        private IEventSource<TEvent, TOffset> eventSource;
        private IEventConsumer<TEvent> consumer;
        private Func<BladeId, IOffsetStorage<TOffset>> offsetStorageFactory;
        private IOffsetInterpreter<TOffset> offsetInterpreter;
        private readonly List<BladeConfigurator<TOffset>> blades = new List<BladeConfigurator<TOffset>>();
        private bool leaderElectionRequired;
        private bool inParallel;
    }
}