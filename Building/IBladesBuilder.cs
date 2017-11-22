using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.Implementations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public interface IBladesBuilder<TOffset>
    {
        [NotNull]
        IBladesBuilder<TOffset> WithBlade([NotNull] string bladeKey, TimeSpan delay);

        [NotNull, ItemNotNull]
        IEnumerable<IBlade> CreateBlades([NotNull] IGlobalTimeProvider globalTimeProvider, [NotNull] IOffsetInterpreter<TOffset> offsetInterpreter, [NotNull] Func<BladeId, IOffsetStorage<TOffset>> createOffsetStorage);
    }
}