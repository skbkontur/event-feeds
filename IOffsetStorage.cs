using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IOffsetStorage<TOffset>
    {
        [NotNull]
        string GetDescription();

        [CanBeNull]
        TOffset Read();

        void Write([CanBeNull] TOffset newOffset);
    }
}