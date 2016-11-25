using MoreLinq;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.Core.EventFeeds.HttpAccess
{
    public class GenericEventFeedHttpHandler : IHttpHandler
    {
        [HttpMethod]
        public void UpdateAndFlush(string eventFeedKey)
        {
            EventFeedsRegistry.GetByKey(eventFeedKey).ExecuteForcedFeeding();
        }

        [HttpMethod]
        public void UpdateAndFlushAll()
        {
            EventFeedsRegistry.GetAll().ForEach(feed => feed.ExecuteForcedFeeding());
        }
    }
}