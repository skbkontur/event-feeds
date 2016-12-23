using System;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    public class BladeId
    {
        public BladeId([NotNull] string key, TimeSpan delay)
        {
            Key = key;
            Delay = delay;
        }

        [NotNull]
        public string Key { get; private set; }

        public TimeSpan Delay { get; private set; }
    }
}