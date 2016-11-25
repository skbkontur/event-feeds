using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeed.Interfaces
{
    public interface IObjectMutationEvent<out TEvent>
    {
        [NotNull]
        TEvent Event { get; }

        void MarkAsProcessed();
        void MarkAsUnprocessed();
    }
}