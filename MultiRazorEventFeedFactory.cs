using GroboContainer.Core;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.Building;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    [PublicAPI]
    public class MultiRazorEventFeedFactory
    {
        public MultiRazorEventFeedFactory([NotNull] IContainer container)
        {
            this.container = container;
        }

        [NotNull]
        public EventFeedsBuilder<TEvent, TOffset> Feed<TEvent, TOffset>([NotNull] string key)
        {
            return container.Create<string, EventFeedsBuilder<TEvent, TOffset>>(key);
        }

        [NotNull]
        public ConcurrentEventFeedsBuilder<TEvent, TOffset> ConcurrentFeed<TEvent, TOffset>([NotNull] string key)
        {
            return container.Create<string, ConcurrentEventFeedsBuilder<TEvent, TOffset>>(key);
        }

        [NotNull]
        private readonly IContainer container;
    }
}