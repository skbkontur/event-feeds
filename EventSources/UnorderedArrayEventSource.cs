using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.CassandraStorageCore.EventLog;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog.Arrays;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventSources
{
    internal class UnorderedArrayEventSource : IEventSource<ArrayEvent, long?>
    {
        public UnorderedArrayEventSource(
            [NotNull] Type businessObjectType,
            [NotNull] ITypeIdentifierProvider typeIdentifierProvider,
            [NotNull] IUnorderedArrayEventLogRepositoryFactory eventLogRepositoryFactory)
        {
            this.businessObjectType = businessObjectType;
            typeIdentifier = typeIdentifierProvider.GetTypeIdentifier(businessObjectType);
            eventLog = eventLogRepositoryFactory.Create(businessObjectType);
        }

        public string GetDescription()
        {
            return string.Format("EventLog based event source: BusinessObjectType: {0}, Type identifier: {1}", businessObjectType.Name, typeIdentifier);
        }

        public EventsQueryResult<ArrayEvent, long?> GetEvents(long? fromOffsetExclusive, long? toOffsetInclusive, int estimatedCount)
        {
            return eventLog.GetEvents<ArrayEvent>(typeIdentifier, fromOffsetExclusive ?? 0, toOffsetInclusive ?? 0, estimatedCount);
        }

        private readonly Type businessObjectType;

        private readonly IUnorderedArrayEventLogRepository eventLog;
        private readonly string typeIdentifier;
    }
}