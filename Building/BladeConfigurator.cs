using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.Implementations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class BladeConfigurator<TOffset>
    {
        public BladeConfigurator([NotNull] string bladeKey, TimeSpan delay)
        {
            bladeId = new BladeId(bladeKey, delay);
        }

        [NotNull]
        public BladeConfigurator<TOffset> WithOffsetFactory(Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage)
        {
            this.createOffsetStorage = createOffsetStorage;
            return this;
        }

        [NotNull]
        public BladeConfigurator<TOffset> WithOffsetInterpreter([NotNull] IOffsetInterpreter<TOffset> offsetInterpreter)
        {
            this.offsetInterpreter = offsetInterpreter;
            return this;
        }

        [NotNull]
        public Blade<TEvent, TOffset> Create<TEvent>([NotNull] IGlobalTimeProvider globalTimeProvider, [NotNull] IEventSource<TEvent, TOffset> eventSource, [NotNull] IEventConsumer<TEvent, TOffset> eventConsumer)
        {
            var offsetStorage = createOffsetStorage(bladeId);
            return new Blade<TEvent, TOffset>(bladeId, globalTimeProvider, eventSource, offsetStorage, offsetInterpreter, eventConsumer);
        }

        private readonly BladeId bladeId;
        private Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage;
        private IOffsetInterpreter<TOffset> offsetInterpreter;
    }
}