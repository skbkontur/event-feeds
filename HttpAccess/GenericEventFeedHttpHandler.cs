﻿using System;
using System.Linq;

using MoreLinq;
using SKBKontur.Catalogue.ServiceLib.HttpHandlers;

namespace SKBKontur.Catalogue.Core.EventFeeds.HttpAccess
{
    public class GenericEventFeedHttpHandler : IHttpHandler
    {
        [HttpMethod]
        public void UpdateAndFlush(string eventFeedKey)
        {
            EventFeedsRegistry.GetByKey(eventFeedKey).ExecuteForcedFeeding(TimeSpan.MaxValue);
        }

        [HttpMethod]
        public void UpdateAndFlushAll(TimeSpan delayUpperBound)
        {
            EventFeedsRegistry.GetAll().Where(feed => feed.Delay <= delayUpperBound).ForEach(feed => feed.ExecuteForcedFeeding(delayUpperBound));
        }
    }
}