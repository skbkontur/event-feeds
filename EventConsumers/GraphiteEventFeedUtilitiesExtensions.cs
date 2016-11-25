using SKBKontur.Catalogue.Core.EventFeed.Interfaces;

namespace SKBKontur.Catalogue.Core.EventFeed.Providers
{
    public static class GraphiteEventFeedUtilitiesExtensions
    {
        public static IEventConsumer<TObjectId> WithReportingErrorCount<TObjectId>(this IEventConsumer<TObjectId> consumer, GraphiteEventFeedUtilities utilities, string graphitePath)
        {
            return utilities.WrapConsumerWithErrorReporter(consumer, graphitePath);
        }
    }
}