using JetBrains.Annotations;

namespace SkbKontur.EventFeeds.Implementations
{
    public class InMemoryOffsetStorage<TOffset> : IOffsetStorage<TOffset>
    {
        public InMemoryOffsetStorage()
            : this(default)
        {
        }

        public InMemoryOffsetStorage([CanBeNull] TOffset initialOffset)
        {
            offset = this.initialOffset = initialOffset;
        }

        [NotNull]
        public string GetDescription()
        {
            return $"InMemoryOffsetStorage<{typeof(TOffset)}> with initialOffset: {initialOffset}";
        }

        [CanBeNull]
        public TOffset Read()
        {
            return offset;
        }

        public void Write([CanBeNull] TOffset newOffset)
        {
            offset = newOffset;
        }

        private TOffset offset;
        private readonly TOffset initialOffset;
    }
}