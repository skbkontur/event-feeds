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
        public CompositeEventFeedsBuilder<TEvent, TOffset> CompositeFeed<TEvent, TOffset>([NotNull] string key)
        {
            return container.Create<string, CompositeEventFeedsBuilder<TEvent, TOffset>>(key);
        }

        [NotNull]
        private readonly IContainer container;
    }
}