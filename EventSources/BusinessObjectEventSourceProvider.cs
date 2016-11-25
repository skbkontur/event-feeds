using JetBrains.Annotations;

using SKBKontur.Catalogue.CassandraStorageCore.EventLog;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog.Arrays;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog.Simple;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl.Interfaces;
using SKBKontur.Catalogue.Core.EventFeed.Providers.BusinessObjectStorage.Implementation;

namespace SKBKontur.Catalogue.Core.EventFeed.Providers.BusinessObjectStorage
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
        public IEventLogEventSource<Event> ForBusinessObject<TBusinessObject>() where TBusinessObject : BusinessObject
        {
            return new UnorderedEventLogEventSource(typeof(TBusinessObject), typeIdentifierProvider, eventLogRepositoryFactory);
        }

        [NotNull]
        public IEventLogEventSource<ArrayEvent> ForBusinessArrayObject<TBusinessObject>() where TBusinessObject : IBusinessArrayObject
        {
            return new UnorderedArrayEventLogEventSource(typeof(TBusinessObject), typeIdentifierProvider, arrayEventLogRepositoryFactory);
        }

        private readonly ITypeIdentifierProvider typeIdentifierProvider;
        private readonly IUnorderedEventLogRepositoryFactory eventLogRepositoryFactory;
        private readonly IUnorderedArrayEventLogRepositoryFactory arrayEventLogRepositoryFactory;
    }
}