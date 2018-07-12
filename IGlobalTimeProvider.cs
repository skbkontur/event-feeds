using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IGlobalTimeProvider
    {
        [NotNull]
        Timestamp GetNowTimestamp();
    }
}