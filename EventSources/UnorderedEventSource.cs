using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.CassandraStorageCore.EventLog;
using SKBKontur.Catalogue.CassandraStorageCore.EventLog.Simple;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.EventSources
{
    internal class UnorderedEventSource : IEventSource<Event, long?>
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
            return $"EventLog based event source: BusinessObjectType: {businessObjectType.Name}, Type identifier: {typeIdentifier}";
        }

        public EventsQueryResult<Event, long?> GetEvents(long? fromOffsetExclusive, long? toOffsetInclusive, int estimatedCount)
        {
            return unorderedEventLog.GetEvents<Event>(typeIdentifier, fromOffsetExclusive ?? 0, toOffsetInclusive ?? 0, estimatedCount);
        }

        private readonly Type businessObjectType;

        private readonly IUnorderedEventLogRepository unorderedEventLog;
        private readonly string typeIdentifier;
    }
}