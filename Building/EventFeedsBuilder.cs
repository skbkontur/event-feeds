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
    public class EventFeedsBuilder<TEvent, TOffset> : ICanStartFeeds
    {
        public EventFeedsBuilder([NotNull] string key,
                                 [NotNull] Lazy<IGlobalTicksHolder> defaultGlobalTicksHolder,
                                 [NotNull] ICatalogueGraphiteClient graphiteClient,
                                 [NotNull] IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.key = key;
            this.defaultGlobalTicksHolder = defaultGlobalTicksHolder;
            this.graphiteClient = graphiteClient;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
        }

        [NotNull]
        public EventFeedsBuilder<TEvent, TOffset> WithGlobalTimeProvider([NotNull] IGlobalTimeProvider globalTimeProvider)
        {
            this.globalTimeProvider = globalTimeProvider;
            return this;
        }

        [NotNull]
        public EventFeedsBuilder<TEvent, TOffset> WithEventSource([NotNull] IEventSource<TEvent, TOffset> eventSource)
        {
            this.eventSource = eventSource;
            return this;
        }

        [NotNull]
        public EventFeedsBuilder<TEvent, TOffset> WithConsumer([NotNull] IEventConsumer<TEvent, TOffset> eventConsumer)
        {
            this.eventConsumer = eventConsumer;
            return this;
        }

        [NotNull]
        public EventFeedsBuilder<TEvent, TOffset> WithOffsetStorageFactory([NotNull] Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage)
        {
            this.offsetStorageFactory = createOffsetStorage;
            return this;
        }

        [NotNull]
        public EventFeedsBuilder<TEvent, TOffset> WithOffsetInterpreter([NotNull] IOffsetInterpreter<TOffset> offsetInterpreter)
        {
            this.offsetInterpreter = offsetInterpreter;
            return this;
        }

        [NotNull]
        public EventFeedsBuilder<TEvent, TOffset> WithBlade([NotNull] string bladeKey, TimeSpan delay)
        {
            blades.Add(new BladeConfigurator<TOffset>(bladeKey, delay));
            return this;
        }

        [NotNull]
        public EventFeedsBuilder<TEvent, TOffset> InParallel()
        {
            this.inParallel = true;
            return this;
        }

        [NotNull]
        public IEventFeedsRunner RunFeeds(TimeSpan delayBetweenIterations)
        {
            var theGlobalTimeProvider = globalTimeProvider ?? new DefaultGlobalTimeProvider(defaultGlobalTicksHolder.Value);
            var eventFeeds = blades.Select(x => x.WithOffsetFactory(offsetStorageFactory)
                                                 .WithOffsetInterpreter(GetOffsetInterpreter())
                                                 .Create(theGlobalTimeProvider, eventSource, eventConsumer))
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

        private bool inParallel;
        private IGlobalTimeProvider globalTimeProvider;
        private IEventSource<TEvent, TOffset> eventSource;
        private IEventConsumer<TEvent, TOffset> eventConsumer;
        private Func<BladeId, IOffsetStorage<TOffset>> offsetStorageFactory;
        private IOffsetInterpreter<TOffset> offsetInterpreter;
        private readonly string key;
        private readonly Lazy<IGlobalTicksHolder> defaultGlobalTicksHolder;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
        private readonly List<BladeConfigurator<TOffset>> blades = new List<BladeConfigurator<TOffset>>();
    }
}