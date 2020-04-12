using System;
using System.Linq;

namespace EventFeeds.Tests
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Max(this TimeSpan[] timeSpans)
        {
            return timeSpans.Aggregate(TimeSpan.MinValue, Max);
        }

        public static TimeSpan Max(TimeSpan first, TimeSpan second)
        {
            return first >= second ? first : second;
        }
    }
}