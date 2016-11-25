using System;
using JetBrains.Annotations;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog.Simple;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventSources
{
    internal class UnorderedEventSource : IEventSource<Event>
    {
        public UnorderedEventSource(
            [NotNull] Type businessObjectType,
            [NotNull] ITypeIdentifierProvider typeIdentifierProvider,
            [NotNull] IUnorderedEventLogRepositoryFactory eventLogRepositoryFactory)
        {
            this.businessObjectType = businessObjectType;
            typeIdentifier = typeIdentifierProvider.GetTypeIdentifier(businessObjectType);
            unorderedEventLog = eventLogRepositoryFactory.Create(businessObjectType);
        }

        public string GetDescription()
        {
            return string.Format("EventLog based event source: BusinessObjectType: {0}, Type identifier: {1}", businessObjectType.Name, typeIdentifier);
        }

        public EventsQueryResult<Event, long> GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int estimatedCount)
        {
            return unorderedEventLog.GetEvents<Event>(typeIdentifier, fromOffsetExclusive, toOffsetInclusive, estimatedCount);
        }

        private readonly Type businessObjectType;

        private readonly IUnorderedEventLogRepository unorderedEventLog;
        private readonly string typeIdentifier;
    }
}