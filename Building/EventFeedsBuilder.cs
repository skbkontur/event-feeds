using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.EventFeeds.Implementations;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

using SkbKontur.Graphite.Client;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class EventFeedsBuilder<TOffset>
    {
        public EventFeedsBuilder(Lazy<IGlobalTicksHolder> defaultGlobalTicksHolder,
                                 IGraphiteClient graphiteClient,
                                 IPeriodicTaskRunner periodicTaskRunner,
                                 IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.defaultGlobalTicksHolder = defaultGlobalTicksHolder;
            this.graphiteClient = graphiteClient;
            this.periodicTaskRunner = periodicTaskRunner;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            bladesBuilders = new List<IBladesBuilder<TOffset>>();
        }

        [NotNull]
        public EventFeedsBuilder<TOffset> WithEventType([NotNull] IBladesBuilder<TOffset> bladesBuilder)
        {
            bladesBuilders.Add(bladesBuilder);
            return this;
        }

        [NotNull]
        public EventFeedsBuilder<TOffset> WithGlobalTimeProvider([NotNull] IGlobalTimeProvider globalTimeProvider)
        {
            this.globalTimeProvider = globalTimeProvider;
            return this;
        }

        [NotNull]
        public EventFeedsBuilder<TOffset> WithOffsetInterpreter([NotNull] IOffsetInterpreter<TOffset> offsetInterpreter)
        {
            this.offsetInterpreter = offsetInterpreter;
            return this;
        }

        [NotNull]
        public EventFeedsBuilder<TOffset> WithOffsetStorageFactory([NotNull] Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage)
        {
            offsetStorageFactory = createOffsetStorage;
            return this;
        }

        [NotNull]
        public EventFeedsBuilder<TOffset> WithSingleLeaderElectionKey([NotNull] string compositeFeedKey)
        {
            this.compositeFeedKey = compositeFeedKey;
            return this;
        }

        [NotNull]
        public IEventFeedsRunner RunFeeds(TimeSpan delayBetweenIterations)
        {
            var theOffsetInterpreter = GetOffsetInterpreter();
            var theGlobalTimeProvider = globalTimeProvider ?? new DefaultGlobalTimeProvider(defaultGlobalTicksHolder.Value);
            var blades = bladesBuilders.SelectMany(x => x.CreateBlades(theGlobalTimeProvider, theOffsetInterpreter, offsetStorageFactory)).ToArray();
            return new EventFeedsRunner(compositeFeedKey, delayBetweenIterations, blades, graphiteClient, periodicTaskRunner, periodicJobRunnerWithLeaderElection);
        }

        [NotNull]
        private IOffsetInterpreter<TOffset> GetOffsetInterpreter()
        {
            if (offsetInterpreter != null)
                return offsetInterpreter;
            if (typeof(TOffset) == typeof(long?))
                return (IOffsetInterpreter<TOffset>)StandardTicksOffsetInterpreter.Instance;
            throw new InvalidProgramStateException($"OffsetInterpreter has not set and for type {typeof(TOffset).FullName} there is no default interpreter");
        }

        private string compositeFeedKey;
        private readonly Lazy<IGlobalTicksHolder> defaultGlobalTicksHolder;
        private readonly IGraphiteClient graphiteClient;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
        private IGlobalTimeProvider globalTimeProvider;
        private Func<BladeId, IOffsetStorage<TOffset>> offsetStorageFactory;
        private IOffsetInterpreter<TOffset> offsetInterpreter;
        private readonly List<IBladesBuilder<TOffset>> bladesBuilders;
    }
}