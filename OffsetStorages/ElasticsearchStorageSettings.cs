using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    public class ElasticsearchStorageSettings
    {
        public ElasticsearchStorageSettings([NotNull] string indexName, [NotNull] string typeName, bool useInternalDataElasticsearch)
        {
            IndexName = indexName;
            TypeName = typeName;
            UseInternalDataElasticsearch = useInternalDataElasticsearch;
        }

        [NotNull]
        public string IndexName { get; private set; }

        [NotNull]
        public string TypeName { get; private set; }

        public bool UseInternalDataElasticsearch { get; private set; }
    }
}