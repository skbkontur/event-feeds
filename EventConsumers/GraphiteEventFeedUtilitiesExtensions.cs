namespace SKBKontur.Catalogue.Core.EventFeeds.EventConsumers
{
    public static class GraphiteEventFeedUtilitiesExtensions
    {
        public static IEventConsumer<TObjectId> WithReportingErrorCount<TObjectId>(this IEventConsumer<TObjectId> consumer, GraphiteEventFeedUtilities utilities, string graphitePath)
        {
            return utilities.WrapConsumerWithErrorReporter(consumer, graphitePath);
        }
    }
}