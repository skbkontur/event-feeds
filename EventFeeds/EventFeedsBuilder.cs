using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.EventFeeds.Implementations;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public class EventFeedsBuilder<TOffset>
    {
        public EventFeedsBuilder(IGlobalTimeProvider globalTimeProvider, IPeriodicJobRunner periodicJobRunner)
        {
            this.globalTimeProvider = globalTimeProvider;
            this.periodicJobRunner = periodicJobRunner;
            bladesBuilders = new List<IBladesBuilder<TOffset>>();
        }

        [NotNull]
        public EventFeedsBuilder<TOffset> WithEventType([NotNull] IBladesBuilder<TOffset> bladesBuilder)
        {
            bladesBuilders.Add(bladesBuilder);
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
            var blades = bladesBuilders.SelectMany(x => x.CreateBlades(globalTimeProvider, theOffsetInterpreter, offsetStorageFactory)).ToArray();
            return new EventFeedsRunner(compositeFeedKey, delayBetweenIterations, blades, periodicJobRunner);
        }

        [NotNull]
        private IOffsetInterpreter<TOffset> GetOffsetInterpreter()
        {
            if (offsetInterpreter != null)
                return offsetInterpreter;

            if (typeof(TOffset) == typeof(long?))
                return (IOffsetInterpreter<TOffset>)StandardTicksOffsetInterpreter.Instance;

            throw new InvalidOperationException($"OffsetInterpreter has not set and for type {typeof(TOffset).FullName} there is no default interpreter");
        }

        private string compositeFeedKey;
        private readonly IGlobalTimeProvider globalTimeProvider;
        private readonly IPeriodicJobRunner periodicJobRunner;
        private Func<BladeId, IOffsetStorage<TOffset>> offsetStorageFactory;
        private IOffsetInterpreter<TOffset> offsetInterpreter;
        private readonly List<IBladesBuilder<TOffset>> bladesBuilders;
    }
}