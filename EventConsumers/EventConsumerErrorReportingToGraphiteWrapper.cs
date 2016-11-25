using SKBKontur.Catalogue.Core.Graphite.Client.StatsD;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventConsumers
{
    public class EventConsumerErrorReportingToGraphiteWrapper<TObjectId> : IEventConsumer<TObjectId>
    {
        public EventConsumerErrorReportingToGraphiteWrapper(
            IEventConsumer<TObjectId> innerConsumer,
            ICatalogueStatsDClient statsDClient,
            string graphitePath
            )
        {
            this.innerConsumer = innerConsumer;
            this.statsDClient = statsDClient;
            this.graphitePath = graphitePath;
        }

        public string GetDescription()
        {
            return innerConsumer.GetDescription() + " with error reporting to graphite";
        }

        public void Initialize()
        {
            innerConsumer.Initialize();
        }

        public void Shutdown()
        {
            innerConsumer.Shutdown();
        }

        public void ProcessEvents(IObjectMutationEvent<TObjectId>[] modificationEvents)
        {
            try
            {
                innerConsumer.ProcessEvents(modificationEvents);
            }
            catch
            {
                statsDClient.Increment(graphitePath);
                throw;
            }
        }

        private readonly IEventConsumer<TObjectId> innerConsumer;
        private readonly ICatalogueStatsDClient statsDClient;
        private readonly string graphitePath;
    }
}