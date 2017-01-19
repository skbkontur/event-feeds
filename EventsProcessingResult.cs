using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public class EventsProcessingResult<TOffset>
    {
        private EventsProcessingResult(bool commitOffset, [CanBeNull] TOffset offsetToCommit)
        {
            if(commitOffset && offsetToCommit == null)
                throw new InvalidProgramStateException("OffsetToCommit is null but commitOffset flag is set to true");
            CommitOffset = commitOffset;
            OffsetToCommit = offsetToCommit;
        }

        public bool CommitOffset { get; private set; }

        [CanBeNull]
        public TOffset OffsetToCommit { get; private set; }

        [NotNull]
        public static EventsProcessingResult<TOffset> DoNotCommitOffset()
        {
            return new EventsProcessingResult<TOffset>(commitOffset : false, offsetToCommit : default(TOffset));
        }

        [NotNull]
        public static EventsProcessingResult<TOffset> DoCommitOffset([NotNull] TOffset offsetToCommit)
        {
            return new EventsProcessingResult<TOffset>(commitOffset : true, offsetToCommit : offsetToCommit);
        }
    }
}