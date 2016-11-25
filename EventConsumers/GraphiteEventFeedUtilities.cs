using GroboContainer.Core;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeed.Interfaces;
using SKBKontur.Catalogue.Core.EventFeed.Providers.Implementation;

namespace SKBKontur.Catalogue.Core.EventFeed.Providers
{
    public class GraphiteEventFeedUtilities
    {
        public GraphiteEventFeedUtilities(
            [NotNull] IContainer container)
        {
            this.container = container;
        }

        public IEventConsumer<TObjectId> WrapConsumerWithErrorReporter<TObjectId>(IEventConsumer<TObjectId> consumer, string graphitePath)
        {
            return container.Create<IEventConsumer<TObjectId>, string, EventConsumerErrorReportingToGraphiteWrapper<TObjectId>>(consumer, graphitePath);
        }

        private readonly IContainer container;
    }
}