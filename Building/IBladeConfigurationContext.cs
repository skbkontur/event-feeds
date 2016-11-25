using System;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl
{
    public interface IBladeConfigurationContext
    {
        [NotNull]
        string BladeKey { get; }

        TimeSpan Delay { get; }
    }
}