using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.EventFeeds.Implementations;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class CompositeEventFeedsBuilder<TEvent, TOffset> : ICanStartFeeds
    {
        public CompositeEventFeedsBuilder([NotNull] string key,
                                          [NotNull] Lazy<IGlobalTicksHolder> defaultGlobalTicksHolder,
                                          [NotNull] ICatalogueGraphiteClient graphiteClient,
                                          [NotNull] IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.key = key;
            this.defaultGlobalTicksHolder = defaultGlobalTicksHolder;
            this.graphiteClient = graphiteClient;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            components = new List<CompositeEventFeedsComponentBuilder<TEvent, TOffset>>();
        }

        [NotNull]
        public CompositeEventFeedsBuilder<TEvent, TOffset> WithGlobalTimeProvider([NotNull] IGlobalTimeProvider globalTimeProvider)
        {
            this.globalTimeProvider = globalTimeProvider;
            return this;
        }

        [NotNull]
        public CompositeEventFeedsBuilder<TEvent, TOffset> WithOffsetInterpreter([NotNull] IOffsetInterpreter<TOffset> offsetInterpreter)
        {
            this.offsetInterpreter = offsetInterpreter;
            return this;
        }

        [NotNull]
        public CompositeEventFeedsBuilder<TEvent, TOffset> WithOffsetStorageFactory([NotNull] Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage)
        {
            offsetStorageFactory = createOffsetStorage;
            return this;
        }

        [NotNull]
        public CompositeEventFeedsBuilder<TEvent, TOffset> WithComponentFeed([NotNull] CompositeEventFeedsComponentBuilder<TEvent, TOffset> builder)
        {
            components.Add(builder);
            return this;
        }

        [NotNull]
        public CompositeEventFeedsBuilder<TEvent, TOffset> InParallel()
        {
            parallel = true;
            return this;
        }

        [NotNull]
        public IEventFeedsRunner RunFeeds(TimeSpan delayBetweenIterations)
        {
            var theOffsetInterpreter = GetOffsetInterpreter();
            var theGlobalTimeProvider = globalTimeProvider ?? new DefaultGlobalTimeProvider(defaultGlobalTicksHolder.Value);
            var eventFeeds = components.SelectMany(feed => feed.Blades.Select(blade => blade.WithOffsetFactory(offsetStorageFactory)
                                                                                            .WithOffsetInterpreter(theOffsetInterpreter)
                                                                                            .Create(theGlobalTimeProvider, feed.EventSource, feed.EventConsumer)))
                                       .ToArray();
            return new EventFeedsRunner<TEvent, TOffset>(key, parallel, delayBetweenIterations, eventFeeds, graphiteClient, periodicJobRunnerWithLeaderElection);
        }

        [NotNull]
        private IOffsetInterpreter<TOffset> GetOffsetInterpreter()
        {
            if(offsetInterpreter != null)
                return offsetInterpreter;
            if(typeof(TOffset) == typeof(long?))
                return (IOffsetInterpreter<TOffset>)StandardTicksOffsetInterpreter.Instance;
            throw new InvalidProgramStateException($"OffsetInterpreter has not set and for type {typeof(TOffset).FullName} there is no default interpreter");
        }

        private readonly string key;
        private readonly Lazy<IGlobalTicksHolder> defaultGlobalTicksHolder;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
        private IGlobalTimeProvider globalTimeProvider;
        private Func<BladeId, IOffsetStorage<TOffset>> offsetStorageFactory;
        private bool parallel;
        private IOffsetInterpreter<TOffset> offsetInterpreter;
        private readonly List<CompositeEventFeedsComponentBuilder<TEvent, TOffset>> components;
    }
}