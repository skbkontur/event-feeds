using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeed.Interfaces
{
    public interface IEventConsumer<in TEvent>
    {
        [NotNull]
        string GetDescription();

        void ProcessEvents([NotNull] IObjectMutationEvent<TEvent>[] modificationEvents);

        void Initialize();
        void Shutdown();
    }
}