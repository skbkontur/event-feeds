using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;

namespace EventFeeds.Tests
{
    public class GlobalTimeProvider : IGlobalTimeProvider
    {
        public Timestamp GetNowTimestamp()
        {
            return Timestamp.Now;
        }
    }
}