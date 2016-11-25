using GroboContainer.Core;

using JetBrains.Annotations;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl
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
        public IEventFeedsBuilder<TEvent, TOffset> Feed2<TEvent, TOffset>([NotNull] string key) where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            return container.Create<string, IEventFeedsBuilder<TEvent, TOffset>>(key);
        }
        
        private readonly IContainer container;
    }
}