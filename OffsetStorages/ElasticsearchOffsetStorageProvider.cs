using Elasticsearch.Net;
using JetBrains.Annotations;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    public class ElasticsearchOffsetStorageProvider
    {
        public ElasticsearchOffsetStorageProvider([NotNull] InternalDataElasticsearchFactory internalDataElasticsearch,
                                               [NotNull] ClientDataElasticsearchFactory clientDataElasticsearch)
        {
            this.internalDataElasticsearch = internalDataElasticsearch;
            this.clientDataElasticsearch = clientDataElasticsearch;
        }

        [NotNull]
        public IOffsetStorage<TOffset> OffsetStorage<TOffset>([NotNull] string key, [NotNull] ElasticsearchStorageSettings settings)
        {
            return new ElasticsearchGenericOffsetInfoStorage<TOffset>(key + "Offset", settings, GetClient(settings));
        }

        private IElasticsearchClient GetClient(ElasticsearchStorageSettings s)
        {
            return s.UseInternalDataElasticsearch ? internalDataElasticsearch.GetClient() : clientDataElasticsearch.GetClient();
        }


        private readonly InternalDataElasticsearchFactory internalDataElasticsearch;
        private readonly ClientDataElasticsearchFactory clientDataElasticsearch;
    }

    public static class ElasticsearchOffsetStorageProviderExtensions
    {
        [NotNull]
        public static IOffsetStorage<TOffset> InternalDataStadardOffsetStorage<TOffset>([NotNull] this ElasticsearchOffsetStorageProvider provider, [NotNull] string key)
        {
            return provider.OffsetStorage<TOffset>(key, new ElasticsearchStorageSettings("EventFeedOffsets".CamelCaseForElasticsearch(), "MultiRazorEventFeedOffset", true));
        }
        
        [NotNull]
        public static IOffsetStorage<TOffset> ClientDataStadardOffsetStorage<TOffset>([NotNull] this ElasticsearchOffsetStorageProvider provider, [NotNull] string key)
        {
            return provider.OffsetStorage<TOffset>(key, new ElasticsearchStorageSettings("EventFeedOffsets".CamelCaseForElasticsearch(), "MultiRazorEventFeedOffset", false));
        }
    }
}