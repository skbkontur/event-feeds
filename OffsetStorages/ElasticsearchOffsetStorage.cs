using Elasticsearch.Net;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;

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
            return $"ElasticsearchOffsetStorage<{typeof(TOffset)}> with IndexName: {indexName}, ElasticType: {elasticTypeName}, Key: {key}";
        }

        public void Write([CanBeNull] TOffset newOffset)
        {
            elasticsearchClient.Index(indexName, elasticTypeName, key, new OffsetStorageElement {Offset = newOffset}).ProcessResponse();
        }

        [CanBeNull]
        public TOffset Read()
        {
            var elasticsearchResponse = elasticsearchClient.Get<GetResponse<OffsetStorageElement>>(indexName, elasticTypeName, key).ProcessResponse().Response;
            if (elasticsearchResponse?.Source != null && elasticsearchResponse.Found)
                return elasticsearchResponse.Source.Offset;
            return GetDefaultOffset();
        }

        [CanBeNull]
        protected virtual TOffset GetDefaultOffset()
        {
            return default(TOffset);
        }

        private const string elasticTypeName = "MultiRazorEventFeedOffset";
        private readonly IElasticsearchClient elasticsearchClient;
        private readonly string indexName;
        private readonly string key;

        private class OffsetStorageElement
        {
            public TOffset Offset { get; set; }
        }
    }
}