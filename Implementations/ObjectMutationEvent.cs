using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    internal class ObjectMutationEvent<TEvent> : IObjectMutationEvent<TEvent>
    {
        public bool? IsProcessed { get; private set; }

        [NotNull]
        public TEvent Event { get; set; }

        public void MarkAsProcessed()
        {
            IsProcessed = true;
        }

        public void MarkAsUnprocessed()
        {
            IsProcessed = false;
        }
    }
}