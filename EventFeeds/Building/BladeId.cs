using System;

using JetBrains.Annotations;

namespace SkbKontur.EventFeeds.Building
{
    [PublicAPI]
    public class BladeId
    {
        public BladeId([NotNull] string bladeKey, TimeSpan delay)
        {
            BladeKey = bladeKey;
            Delay = delay;
        }

        [NotNull]
        public string BladeKey { get; }

        public TimeSpan Delay { get; }

        public override string ToString()
        {
            return $"[BladeKey: {BladeKey}, Delay: {Delay}]";
        }
    }
}