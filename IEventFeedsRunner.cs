using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IEventFeedsRunner
    {
        [NotNull]
        IEventFeed[] RunningFeeds { get; }

        void Stop();
    }
}