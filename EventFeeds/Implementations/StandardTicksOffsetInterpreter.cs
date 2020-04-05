using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.EventFeeds.Implementations
{
    public class StandardTicksOffsetInterpreter : IOffsetInterpreter<long?>
    {
        private StandardTicksOffsetInterpreter()
        {
        }

        [NotNull]
        public string Format([CanBeNull] long? offset)
        {
            if (offset == null)
                return "(null)";
            if (offset < Timestamp.MinValue.Ticks || offset > Timestamp.MaxValue.Ticks)
                return offset.ToString();
            return new Timestamp(offset.Value).ToString();
        }

        [CanBeNull]
        public Timestamp GetTimestampFromOffset([CanBeNull] long? offset)
        {
            if (offset == null || offset < Timestamp.MinValue.Ticks || offset > Timestamp.MaxValue.Ticks)
                return null;
            return new Timestamp(offset.Value);
        }

        [NotNull]
        public long? GetMaxOffsetForTimestamp([NotNull] Timestamp timestamp)
        {
            return timestamp.Ticks;
        }

        public int Compare([CanBeNull] long? x, [CanBeNull] long? y)
        {
            if (!x.HasValue && !y.HasValue)
                return 0;
            if (!x.HasValue)
                return -1;
            if (!y.HasValue)
                return 1;
            return x.Value.CompareTo(y.Value);
        }

        public static readonly StandardTicksOffsetInterpreter Instance = new StandardTicksOffsetInterpreter();
    }
}