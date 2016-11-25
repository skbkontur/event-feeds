using GroboContainer.Core;
using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventConsumers
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