using JetBrains.Annotations;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public interface IOffsetStorage<TOffset>
    {
        [NotNull]
        string GetDescription();

        [CanBeNull]
        TOffset Read();

        void Write([CanBeNull] TOffset newOffset);
    }
}