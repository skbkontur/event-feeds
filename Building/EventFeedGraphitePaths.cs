using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Building
{
    internal class EventFeedGraphitePaths
    {
        public EventFeedGraphitePaths([CanBeNull] string actualizationLag)
        {
            ActualizationLag = actualizationLag;
        }

        [CanBeNull]
        public string ActualizationLag { get; private set; }
    }
}