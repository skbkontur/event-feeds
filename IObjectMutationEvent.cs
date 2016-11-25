using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds
{
    public interface IObjectMutationEvent<out TEvent>
    {
        [NotNull]
        TEvent Event { get; }

        void MarkAsProcessed();
        void MarkAsUnprocessed();
    }
}