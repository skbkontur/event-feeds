using System;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public interface ICanStartFeeds
    {
        [NotNull]
        IEventFeedsRunner RunFeeds(TimeSpan delayBetweenIterations);
    }
}