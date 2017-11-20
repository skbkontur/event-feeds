using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class CompositeEventFeedsComponentBuilder<TEvent, TOffset>
    {
        public CompositeEventFeedsComponentBuilder([NotNull] IEventSource<TEvent, TOffset> eventSource, [NotNull] IEventConsumer<TEvent, TOffset> eventConsumer)
        {
            EventSource = eventSource;
            EventConsumer = eventConsumer;
            Blades = new List<BladeConfigurator<TOffset>>();
        }

        [NotNull]
        public CompositeEventFeedsComponentBuilder<TEvent, TOffset> WithBlade([NotNull] string bladeKey, TimeSpan delay)
        {
            Blades.Add(new BladeConfigurator<TOffset>(bladeKey, delay));
            return this;
        }

        [NotNull]
        public IEventSource<TEvent, TOffset> EventSource { get; }

        [NotNull]
        public IEventConsumer<TEvent, TOffset> EventConsumer { get; }

        [NotNull, ItemNotNull]
        public List<BladeConfigurator<TOffset>> Blades { get; }
    }
}