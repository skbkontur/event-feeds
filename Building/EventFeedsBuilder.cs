using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.EventFeeds.Firing;
using SKBKontur.Catalogue.Core.EventFeeds.Implementations;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class EventFeedsBuilder<TEvent, TOffset> : IEventFeedsBuilder<TEvent, TOffset>
    {
        public EventFeedsBuilder([NotNull] string key,
                                 [NotNull] IGlobalTicksHolder globalTicksHolder,
                                 [NotNull] ICatalogueGraphiteClient graphiteClient,
                                 [NotNull] IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.key = key;
            this.globalTicksHolder = globalTicksHolder;
            this.graphiteClient = graphiteClient;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> WithEventSource([NotNull] IEventSource<TEvent, TOffset> eventSource)
        {
            this.eventSource = eventSource;
            return this;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> WithConsumer([NotNull] IEventConsumer<TEvent, TOffset> eventConsumer)
        {
            this.eventConsumer = eventConsumer;
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
        public IEventFeedsBuilder<TEvent, TOffset> InParallel()
        {
            this.inParallel = true;
            return this;
        }

        [NotNull]
        public IEventFeedsRunner RunFeeds(TimeSpan delayBetweenIterations)
        {
            var eventFeeds = blades.Select(x => x.WithOffsetFactory(offsetStorageFactory)
                                                 .WithOffsetInterpreter(GetOffsetInterpreter())
                                                 .Create(globalTicksHolder, eventSource, eventConsumer))
                                   .ToList();
            return new EventFeedsRunner<TEvent, TOffset>(key, inParallel, delayBetweenIterations, eventFeeds, graphiteClient, periodicJobRunnerWithLeaderElection);
        }

        [NotNull]
        private IOffsetInterpreter<TOffset> GetOffsetInterpreter()
        {
            if(offsetInterpreter != null)
                return offsetInterpreter;
            if(typeof(TOffset) == typeof(long?))
                return (IOffsetInterpreter<TOffset>)StandardTicksOffsetInterpreter.Instance;
            throw new InvalidProgramStateException(string.Format("OffsetInterpreter has not set and for type {0} there is no default interpreter", typeof(TOffset).FullName));
        }

        private readonly string key;
        private readonly IGlobalTicksHolder globalTicksHolder;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
        private IEventSource<TEvent, TOffset> eventSource;
        private IEventConsumer<TEvent, TOffset> eventConsumer;
        private Func<BladeId, IOffsetStorage<TOffset>> offsetStorageFactory;
        private IOffsetInterpreter<TOffset> offsetInterpreter;
        private bool inParallel;
        private readonly List<BladeConfigurator<TOffset>> blades = new List<BladeConfigurator<TOffset>>();
    }
}