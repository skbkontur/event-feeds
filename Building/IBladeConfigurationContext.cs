using System;
using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public interface IBladeConfigurationContext
    {
        [NotNull]
        string BladeKey { get; }

        TimeSpan Delay { get; }
    }
}