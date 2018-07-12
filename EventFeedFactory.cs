using GroboContainer.Core;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.Building;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    [PublicAPI]
    public class EventFeedFactory
    {
        public EventFeedFactory([NotNull] IContainer container)
        {
            this.container = container;
        }

        [NotNull]
        public EventFeedsBuilder<TOffset> WithOffsetType<TOffset>()
        {
            return container.Create<EventFeedsBuilder<TOffset>>();
        }

        private readonly IContainer container;
    }
}