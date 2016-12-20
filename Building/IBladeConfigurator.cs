using System;
using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public interface IBladeConfigurator<TOffset>
    {
        [NotNull]
        IBladeConfigurator<TOffset> WithDelay(TimeSpan delay);
    }
}