using JetBrains.Annotations;

using Elasticsearch.Net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.EventFeed.Interfaces;
using SKBKontur.Catalogue.Core.EventFeed.Providers.Elasticsearch.Implementation;

namespace SKBKontur.Catalogue.Core.EventFeed.Providers.Elasticsearch
{
    public class ElasticsearchEventFeedProviders
    {
        public ElasticsearchEventFeedProviders([NotNull] InternalDataElasticsearchFactory internalDataElasticsearch,
                                               [NotNull] ClientDataElasticsearchFactory clientDataElasticsearch)
        {
            this.internalDataElasticsearch = internalDataElasticsearch;
            this.clientDataElasticsearch = clientDataElasticsearch;
        }

        [NotNull]
        public IOffsetStorage<TOffset> OffsetStorage<TOffset>([NotNull] string key, [NotNull] ElasticsearchStorageSettings settings)
        {
            return new ElasticsearchGenericOffsetInfoStorage<TOffset>(key, settings, GetClient(settings));
        }

        private IElasticsearchClient GetClient(ElasticsearchStorageSettings s)
        {
            return s.UseInternalDataElasticsearch ? internalDataElasticsearch.GetClient() : clientDataElasticsearch.GetClient();
        }


        private readonly InternalDataElasticsearchFactory internalDataElasticsearch;
        private readonly ClientDataElasticsearchFactory clientDataElasticsearch;
    }
}