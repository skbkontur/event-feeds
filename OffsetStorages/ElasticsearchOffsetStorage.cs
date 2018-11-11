using Elasticsearch.Net;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;
using SKBKontur.Catalogue.Objects.Json;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    public class ElasticsearchOffsetStorage<TOffset> : IOffsetStorage<TOffset>
    {
        public ElasticsearchOffsetStorage([NotNull] IElasticLowLevelClient elasticClient, [NotNull] string key, [NotNull] string indexName = "event-feed-offsets")
        {
            this.elasticClient = elasticClient;
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
            var payload = new OffsetStorageElement {Offset = newOffset};
            elasticClient.Index<StringResponse>(indexName, elasticTypeName, key, PostData.String(payload.ToJson())).EnsureSuccess();
        }

        [CanBeNull]
        public TOffset Read()
        {
            var elasticResponse = elasticClient.Get<StringResponse>(indexName, elasticTypeName, key, allowNotFoundStatusCode).EnsureSuccess().Body?.FromJson<GetResponse<OffsetStorageElement>>();
            if (elasticResponse?.Source != null && elasticResponse.Found)
                return elasticResponse.Source.Offset;
            return GetDefaultOffset();
        }

        [CanBeNull]
        protected virtual TOffset GetDefaultOffset()
        {
            return default(TOffset);
        }

        private const string elasticTypeName = "MultiRazorEventFeedOffset";
        private readonly IElasticLowLevelClient elasticClient;
        private readonly string indexName;
        private readonly string key;

        private readonly GetRequestParameters allowNotFoundStatusCode = new GetRequestParameters
            {
                RequestConfiguration = new RequestConfiguration {AllowedStatusCodes = new[] {404}}
            };

        private class OffsetStorageElement
        {
            public TOffset Offset { get; set; }
        }
    }
}