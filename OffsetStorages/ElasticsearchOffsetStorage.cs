using Elasticsearch.Net;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;
using SKBKontur.Catalogue.Objects;

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
            return string.Format("ElasticsearchOffsetStorage<{0}> with IndexName: {1}, ElasticType: {2}, Key: {3} ", typeof(TOffset), indexName, elasticTypeName, key);
        }

        public void Write([CanBeNull] TOffset newOffset)
        {
            elasticsearchClient.Index(indexName, elasticTypeName, key, new OffsetStorageElement {Offset = newOffset}).ProcessResponse();
        }

        [CanBeNull]
        public TOffset Read()
        {
            var elasticsearchResponse = elasticsearchClient.Get<GetResponse<OffsetStorageElement>>(indexName, elasticTypeName, key).ProcessResponse();
            return elasticsearchResponse.With(x => x.Response).If(x => x.Found).With(x => x.Source).Return(x => x.Offset, GetDefaultOffset());
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