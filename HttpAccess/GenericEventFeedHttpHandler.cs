using MoreLinq;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.Core.EventFeeds.HttpAccess
{
    public class GenericEventFeedHttpHandler : IHttpHandler
    {
        public GenericEventFeedHttpHandler(IEventFeedRegistry eventFeedRegistry)
        {
            this.eventFeedRegistry = eventFeedRegistry;
        }

        [HttpMethod]
        public void UpdateAndFlush(string eventFeedKey)
        {
            eventFeedRegistry.GetByKey(eventFeedKey).ExecuteForcedFeeding();
        }

        [HttpMethod]
        public void UpdateAndFlushAll()
        {
            eventFeedRegistry.GetAll().ForEach(feed => feed.ExecuteForcedFeeding());
        }

        private readonly IEventFeedRegistry eventFeedRegistry;
    }
}