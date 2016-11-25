using JetBrains.Annotations;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public interface IUnprocessedEventsBladeConfigurator<TEvent> where TEvent : GenericEvent
    {
        [NotNull]
        IUnprocessedEventsBladeConfigurator<TEvent> AndLeaderElectionRequired();
    }
}