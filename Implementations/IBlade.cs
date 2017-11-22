using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.Building;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public interface IBlade : IEventFeed
    {
        [NotNull]
        BladeId BladeId { get; }

        void Initialize();
        void Shutdown();
        void ExecuteFeeding();

        [CanBeNull]
        Timestamp GetCurrentGlobalOffsetTimestamp();
    }
}