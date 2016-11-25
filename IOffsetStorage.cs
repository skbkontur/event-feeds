using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeed.Interfaces
{
    public interface IOffsetStorage<TOffset>
    {
        [NotNull]
        string GetDescription();

        [CanBeNull]
        TOffset Read([CanBeNull] string key);

        void Write([CanBeNull] string key, [CanBeNull] TOffset minEventInfo);
    }
}