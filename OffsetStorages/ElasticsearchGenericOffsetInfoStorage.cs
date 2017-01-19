using Elasticsearch.Net;

using JetBrains.Annotations;

using log4net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Get;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.Json;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    internal class ElasticsearchGenericOffsetInfoStorage<TOffset> : IOffsetStorage<TOffset>
    {
        public ElasticsearchGenericOffsetInfoStorage(
            [NotNull] string key,
            [NotNull] ElasticsearchStorageSettings settings,
            [NotNull] IElasticsearchClient elasticsearchClient)
        {
            this.key = key;
            this.settings = settings;
            this.elasticsearchClient = elasticsearchClient;
        }

        public string GetDescription()
        {
            return string.Format("Offset in elasticsearch index storage: Offset type: {3} Index: {0}, Type: {1}, Key: {2} ", settings.IndexName, settings.TypeName, key, typeof(TOffset).Name);
        }

        public void Write(TOffset newOffset)
        {
            elasticsearchClient.Index(
                settings.IndexName,
                settings.TypeName,
                key,
                new EventInfoStorageElement
                    {
                        Offset = newOffset
                    }
                ).ProcessResponse();
        }

        public TOffset Read()
        {
            var elasticsearchResponse = elasticsearchClient.Get<GetResponse<EventInfoStorageElement>>(settings.IndexName, settings.TypeName, key).ProcessResponse();
            logger.InfoFormat("OffsetStorage got elasticsearch response: {0}", elasticsearchResponse.ToPrettyJson());
            return elasticsearchResponse
                .With(x => x.Response)
                .If(x => x.Found)
                .With(x => x.Source)
                .Return(x => x.Offset, default(TOffset));
        }

        private readonly ILog logger = LogManager.GetLogger(typeof(ElasticsearchGenericOffsetInfoStorage<>));
        private readonly string key;
        private readonly ElasticsearchStorageSettings settings;
        private readonly IElasticsearchClient elasticsearchClient;

        private class EventInfoStorageElement
        {
            public TOffset Offset { get; set; }
        }
    }
}