using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.Firing;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public interface ICanStartFeeds
    {
        [NotNull]
        IEventFeedsRunner RunFeeds(TimeSpan delayBetweenIterations);
    }
}