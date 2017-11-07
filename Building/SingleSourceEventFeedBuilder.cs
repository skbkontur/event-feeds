using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class SingleSourceEventFeedBuilder<TEvent, TOffset>
    {
        public SingleSourceEventFeedBuilder([NotNull] IEventSource<TEvent, TOffset> eventSource, [NotNull] IEventConsumer<TEvent, TOffset> eventConsumer)
        {
            EventSource = eventSource;
            EventConsumer = eventConsumer;
            blades = new List<BladeConfigurator<TOffset>>();
        }

        [NotNull]
        public SingleSourceEventFeedBuilder<TEvent, TOffset> WithBlade([NotNull] string bladeKey, TimeSpan delay)
        {
            blades.Add(new BladeConfigurator<TOffset>(bladeKey, delay));
            return this;
        }

        [NotNull]
        public IEventSource<TEvent, TOffset> EventSource { get; }

        [NotNull]
        public IEventConsumer<TEvent, TOffset> EventConsumer { get; }

        [NotNull, ItemNotNull]
        private readonly List<BladeConfigurator<TOffset>> blades;

        public IReadOnlyCollection<BladeConfigurator<TOffset>> Blades { get { return blades.ToArray(); } }
    }
}