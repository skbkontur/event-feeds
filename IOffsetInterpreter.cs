using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IOffsetInterpreter<TOffset> : IComparer<TOffset>
    {
        [NotNull]
        string Format([CanBeNull] TOffset offset);

        [CanBeNull]
        Timestamp GetTimestampFromOffset([CanBeNull] TOffset offset);

        [NotNull]
        TOffset GetMaxOffsetForTimestamp([NotNull] Timestamp timestamp);
    }
}