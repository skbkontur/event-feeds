using JetBrains.Annotations;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog.Arrays;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog.Simple;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventSources
{
    public class BusinessObjectEventSourceProvider
    {
        public BusinessObjectEventSourceProvider(
            [NotNull] ITypeIdentifierProvider typeIdentifierProvider,
            [NotNull] IUnorderedEventLogRepositoryFactory eventLogRepositoryFactory,
            [NotNull] IUnorderedArrayEventLogRepositoryFactory arrayEventLogRepositoryFactory
            )
        {
            this.typeIdentifierProvider = typeIdentifierProvider;
            this.eventLogRepositoryFactory = eventLogRepositoryFactory;
            this.arrayEventLogRepositoryFactory = arrayEventLogRepositoryFactory;
        }

        [NotNull]
        public IEventSource<Event, long> ForBusinessObject<TBusinessObject>() where TBusinessObject : BusinessObject
        {
            return new UnorderedEventSource(typeof(TBusinessObject), typeIdentifierProvider, eventLogRepositoryFactory);
        }

        [NotNull]
        public IEventSource<ArrayEvent, long> ForBusinessArrayObject<TBusinessObject>() where TBusinessObject : IBusinessArrayObject
        {
            return new UnorderedArrayEventSource(typeof(TBusinessObject), typeIdentifierProvider, arrayEventLogRepositoryFactory);
        }

        private readonly ITypeIdentifierProvider typeIdentifierProvider;
        private readonly IUnorderedEventLogRepositoryFactory eventLogRepositoryFactory;
        private readonly IUnorderedArrayEventLogRepositoryFactory arrayEventLogRepositoryFactory;
    }
}