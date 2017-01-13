using GroboContainer.Core;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.Building;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    [PublicAPI]
    public class MultiRazorEventFeedFactory
    {
        public MultiRazorEventFeedFactory(
            [NotNull] IContainer container)
        {
            this.container = container;
        }

        [NotNull]
        public IEventFeedsBuilder<TEvent, TOffset> Feed<TEvent, TOffset>([NotNull] string key)
        {
            return container.Create<string, IEventFeedsBuilder<TEvent, TOffset>>(key);
        }

        private readonly IContainer container;
    }
}