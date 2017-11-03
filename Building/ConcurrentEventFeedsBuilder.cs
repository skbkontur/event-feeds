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
    public class ConcurrentEventFeedsBuilder<TEvent, TOffset> : ICanStartFeeds
    {
        public ConcurrentEventFeedsBuilder([NotNull] string key,
                                           [NotNull] Lazy<IGlobalTicksHolder> defaultGlobalTicksHolder,
                                           [NotNull] ICatalogueGraphiteClient graphiteClient,
                                           [NotNull] IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.key = key;
            this.defaultGlobalTicksHolder = defaultGlobalTicksHolder;
            this.graphiteClient = graphiteClient;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            concurringFeeds = new List<SingleSourceEventFeedBuilder<TEvent, TOffset>>();
        }

        [NotNull]
        public ConcurrentEventFeedsBuilder<TEvent, TOffset> WithGlobalTimeProvider([NotNull] IGlobalTimeProvider globalTimeProvider)
        {
            this.globalTimeProvider = globalTimeProvider;
            return this;
        }

        [NotNull]
        public ConcurrentEventFeedsBuilder<TEvent, TOffset> WithOffsetInterpreter([NotNull] IOffsetInterpreter<TOffset> offsetInterpreter)
        {
            this.offsetInterpreter = offsetInterpreter;
            return this;
        }

        [NotNull]
        public ConcurrentEventFeedsBuilder<TEvent, TOffset> WithConsumer([NotNull] IEventConsumer<TEvent, TOffset> eventConsumer)
        {
            this.eventConsumer = eventConsumer;
            return this;
        }

        [NotNull]
        public ConcurrentEventFeedsBuilder<TEvent, TOffset> WithOffsetStorageFactory([NotNull] Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage)
        {
            offsetStorageFactory = createOffsetStorage;
            return this;
        }

        [NotNull]
        public ConcurrentEventFeedsBuilder<TEvent, TOffset> WithConcurringFeed([NotNull] SingleSourceEventFeedBuilder<TEvent, TOffset> builder)
        {
            concurringFeeds.Add(builder);
            return this;
        }

        [NotNull]
        public IEventFeedsRunner RunFeeds(TimeSpan delayBetweenIterations)
        {
            var theGlobalTimeProvider = globalTimeProvider ?? new DefaultGlobalTimeProvider(defaultGlobalTicksHolder.Value);
            var eventFeeds = concurringFeeds.SelectMany(feed => feed.Blades.Select(blade => blade.WithOffsetFactory(offsetStorageFactory)
                                                                                                 .WithOffsetInterpreter(GetOffsetInterpreter())
                                                                                                 .Create(theGlobalTimeProvider, feed.EventSource, eventConsumer)))
                                            .ToList();
            return new EventFeedsRunner<TEvent, TOffset>(key, false, delayBetweenIterations, eventFeeds, graphiteClient, periodicJobRunnerWithLeaderElection);
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
        private readonly Lazy<IGlobalTicksHolder> defaultGlobalTicksHolder;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
        private IGlobalTimeProvider globalTimeProvider;
        private IEventConsumer<TEvent, TOffset> eventConsumer;
        private Func<BladeId, IOffsetStorage<TOffset>> offsetStorageFactory;

        [CanBeNull]
        private IOffsetInterpreter<TOffset> offsetInterpreter;

        private readonly List<SingleSourceEventFeedBuilder<TEvent, TOffset>> concurringFeeds;
    }
}