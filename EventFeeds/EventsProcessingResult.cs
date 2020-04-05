using System;

using JetBrains.Annotations;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public class EventsProcessingResult<TOffset>
    {
        private EventsProcessingResult(bool commitOffset, [CanBeNull] TOffset offsetToCommit)
        {
            if (commitOffset && offsetToCommit == null)
                throw new InvalidOperationException("OffsetToCommit is null but commitOffset flag is set to true");
            CommitOffset = commitOffset;
            OffsetToCommit = offsetToCommit;
        }

        public bool CommitOffset { get; }

        [CanBeNull]
        public TOffset OffsetToCommit { get; }

        [NotNull]
        public TOffset GetOffsetToCommit()
        {
            if (OffsetToCommit == null)
                throw new InvalidOperationException("OffsetToCommit is null");
            return OffsetToCommit;
        }

        public override string ToString()
        {
            return $"CommitOffset: {CommitOffset}, OffsetToCommit: {OffsetToCommit}";
        }

        [NotNull]
        public static EventsProcessingResult<TOffset> DoNotCommitOffset()
        {
            return new EventsProcessingResult<TOffset>(commitOffset : false, offsetToCommit : default);
        }

        [NotNull]
        public static EventsProcessingResult<TOffset> DoCommitOffset([NotNull] TOffset offsetToCommit)
        {
            return new EventsProcessingResult<TOffset>(commitOffset : true, offsetToCommit : offsetToCommit);
        }
    }
}