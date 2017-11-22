using System;

using MoreLinq;

using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.Core.EventFeeds.HttpAccess
{
    // todo (andrew, 22.11.2017): get rid of this dirty static hack (EventFeedsRegistry)
    public sealed class EventFeedHttpHandler : IHttpHandler
    {
        [HttpMethod]
        public void UpdateAndFlushAll(TimeSpan delayUpperBound)
        {
            EventFeedsRegistry.GetAll().ForEach(feed => feed.ExecuteForcedFeeding(delayUpperBound));
        }
    }
}