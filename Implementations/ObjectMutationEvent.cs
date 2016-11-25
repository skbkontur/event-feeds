using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeed.Interfaces;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl.Implementation
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