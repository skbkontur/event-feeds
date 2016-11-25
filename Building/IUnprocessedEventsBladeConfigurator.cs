using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl
{
    public interface IUnprocessedEventsBladeConfigurator<TEvent> where TEvent : GenericEvent
    {
        [NotNull]
        IUnprocessedEventsBladeConfigurator<TEvent> AndLeaderElectionRequired();
    }
}