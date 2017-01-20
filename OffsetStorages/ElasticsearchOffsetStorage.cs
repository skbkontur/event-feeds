using Elasticsearch.Net;

using JetBrains.Annotations;

using log4net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.Json;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    public class ElasticsearchOffsetStorage<TOffset> : IOffsetStorage<TOffset>
    {
        public ElasticsearchOffsetStorage([NotNull] IElasticsearchClient elasticsearchClient, [NotNull] string key, [NotNull] string indexName = "event-feed-offsets")
        {
            this.elasticsearchClient = elasticsearchClient;
            this.indexName = indexName;
            this.key = key;
        }

        [NotNull]
        public string GetDescription()
        {
            return string.Format("Offset in elasticsearch index storage: Offset type: {0} IndexName: {1}, ElasticType: {2}, Key: {3} ", typeof(TOffset), indexName, elasticTypeName, key);
        }

        public void Write([CanBeNull] TOffset newOffset)
        {
            elasticsearchClient.Index(indexName, elasticTypeName, key, new OffsetStorageElement {Offset = newOffset}).ProcessResponse();
        }

        [CanBeNull]
        public TOffset Read()
        {
            var elasticsearchResponse = elasticsearchClient.Get<GetResponse<OffsetStorageElement>>(indexName, elasticTypeName, key).ProcessResponse();
            logger.InfoFormat("OffsetStorage got elasticsearch response: {0}", elasticsearchResponse.ToPrettyJson());
            return elasticsearchResponse.With(x => x.Response).If(x => x.Found).With(x => x.Source).Return(x => x.Offset, GetDefaultOffset());
        }

        [CanBeNull]
        protected virtual TOffset GetDefaultOffset()
        {
            return default(TOffset);
        }

        private const string elasticTypeName = "MultiRazorEventFeedOffset";
        private readonly ILog logger = LogManager.GetLogger(typeof(ElasticsearchOffsetStorage<>));
        private readonly IElasticsearchClient elasticsearchClient;
        private readonly string indexName;
        private readonly string key;

        private class OffsetStorageElement
        {
            public TOffset Offset { get; set; }
        }
    }
}