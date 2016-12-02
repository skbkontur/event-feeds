using JetBrains.Annotations;
using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeeds.Implementations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    internal class UnprocessedEventsBladeConfigurator<TEvent> where TEvent : GenericEvent
    {
        public UnprocessedEventsBladeConfigurator([NotNull] string key)
        {
            this.key = key;
        }

        [NotNull]
        public UnprocessedEventsBladeConfigurator<TEvent> WithUnprocessedEventsStorage([NotNull] IUnprocessedEventsStorage<TEvent> storage)
        {
            this.unprocessedEventsStorage = storage;
            return this;
        }

        [NotNull]
        public UnprocessedEventsBladeConfigurator<TEvent> AndLeaderElectionBehavior(bool leaderElectionRequired)
        {
            this.leaderElectionRequired = leaderElectionRequired;
            return this;
        }

        [NotNull]
        public UnprocessedEventsFeed<TEvent> Create([NotNull] IEventConsumer<TEvent> eventConsumer, [NotNull] IGlobalTicksHolder globalTicksHolder)
        {
            return new UnprocessedEventsFeed<TEvent>(key, unprocessedEventsStorage, eventConsumer, globalTicksHolder, leaderElectionRequired);
        }

        private readonly string key;
        private IUnprocessedEventsStorage<TEvent> unprocessedEventsStorage;
        private bool leaderElectionRequired;
    }
}