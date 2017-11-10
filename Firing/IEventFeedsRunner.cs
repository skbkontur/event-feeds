using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Firing
{
    public interface IEventFeedsRunner
    {
        [NotNull]
        ICompositeEventFeed RunningFeed { get; }

        void StopFeed();
    }
}