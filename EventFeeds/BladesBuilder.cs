using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using SkbKontur.EventFeeds.Implementations;

using Vostok.Logging.Abstractions;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public static class BladesBuilder
    {
        [NotNull]
        public static BladesBuilder<TEvent, TOffset> New<TEvent, TOffset>([NotNull] IEventSource<TEvent, TOffset> eventSource,
                                                                          [NotNull] IEventConsumer<TEvent, TOffset> eventConsumer,
                                                                          [NotNull] ILog logger)
        {
            return new BladesBuilder<TEvent, TOffset>(eventSource, eventConsumer, logger);
        }
    }

    [PublicAPI]
    public class BladesBuilder<TEvent, TOffset> : IBladesBuilder<TOffset>
    {
        public BladesBuilder([NotNull] IEventSource<TEvent, TOffset> eventSource,
                             [NotNull] IEventConsumer<TEvent, TOffset> eventConsumer,
                             [NotNull] ILog logger)
        {
            this.eventSource = eventSource;
            this.eventConsumer = eventConsumer;
            this.logger = logger;
        }

        [NotNull]
        public IBladesBuilder<TOffset> WithBlade([NotNull] string bladeKey, TimeSpan delay)
        {
            bladeIds.Add(new BladeId(bladeKey, delay));
            return this;
        }

        [NotNull, ItemNotNull]
        public IEnumerable<IBlade> CreateBlades([NotNull] IGlobalTimeProvider globalTimeProvider,
                                                [NotNull] IOffsetInterpreter<TOffset> offsetInterpreter,
                                                [NotNull] Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage)
        {
            foreach (var bladeId in bladeIds)
            {
                var offsetStorage = createOffsetStorage(bladeId);
                yield return new Blade<TEvent, TOffset>(bladeId, globalTimeProvider, eventSource, offsetStorage, offsetInterpreter, eventConsumer, logger);
            }
        }

        private readonly IEventSource<TEvent, TOffset> eventSource;
        private readonly IEventConsumer<TEvent, TOffset> eventConsumer;
        private readonly ILog logger;
        private readonly List<BladeId> bladeIds = new List<BladeId>();
    }
}