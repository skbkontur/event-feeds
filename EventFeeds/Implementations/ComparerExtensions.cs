using System.Collections.Generic;

using JetBrains.Annotations;

namespace SkbKontur.EventFeeds.Implementations
{
    internal static class ComparerExtensions
    {
        [CanBeNull]
        public static T Max<T>(this IComparer<T> comparer, [CanBeNull] T a, [CanBeNull] T b)
        {
            return comparer.Compare(a, b) < 0 ? b : a;
        }
    }
}