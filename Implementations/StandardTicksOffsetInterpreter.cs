using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class StandardTicksOffsetInterpreter : IOffsetInterpreter<long>
    {
        private StandardTicksOffsetInterpreter()
        {
        }

        public int Compare(long x, long y)
        {
            return Comparer<long>.Default.Compare(x, y);
        }

        public string Format(long offset)
        {
            if(offset < Timestamp.MinValue.Ticks || offset > Timestamp.MaxValue.Ticks)
                return offset.ToString();
            return new Timestamp(offset).ToString();
        }

        [CanBeNull]
        public Timestamp ToTimestamp(long offset)
        {
            if(offset < Timestamp.MinValue.Ticks || offset > Timestamp.MaxValue.Ticks)
                return null;
            return new Timestamp(offset);
        }

        public long FromTimestamp([CanBeNull] Timestamp timestamp)
        {
            return timestamp == null ? 0 : timestamp.Ticks;
        }

        public static readonly StandardTicksOffsetInterpreter Instance = new StandardTicksOffsetInterpreter();
    }
}