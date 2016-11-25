using JetBrains.Annotations;

using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeed.Interfaces;
using SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl.Implementation;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl
{
    internal class UnprocessedEventsBladeConfigurator<TEvent> : IUnprocessedEventsBladeConfigurator<TEvent> where TEvent : GenericEvent
    {
        public UnprocessedEventsBladeConfigurator([NotNull] string key)
        {
            this.key = key;
        }

        [NotNull]
        public IUnprocessedEventsBladeConfigurator<TEvent> WithUnprocessedEventsStorage([NotNull] IUnprocessedEventsStorage<TEvent> storage)
        {
            this.unprocessedEventsStorage = storage;
            return this;
        }

        [NotNull]
        public IUnprocessedEventsBladeConfigurator<TEvent> AndLeaderElectionRequired()
        {
            leaderElectionRequired = true;
            return this;
        }

        [NotNull]
        public UnprocessedEventFeed<TEvent> Create([NotNull] IEventConsumer<TEvent> eventConsumer, [NotNull] IGlobalTicksHolder globalTicksHolder)
        {
            return new UnprocessedEventFeed<TEvent>(key, unprocessedEventsStorage, eventConsumer, globalTicksHolder, leaderElectionRequired);
        }

        private readonly string key;
        private IUnprocessedEventsStorage<TEvent> unprocessedEventsStorage;
        private bool leaderElectionRequired;
    }
}