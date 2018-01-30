using JetBrains.Annotations;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    internal class BusinessObjectStorageOffsetContainer<TOffset, TBusinessObject> : IOffsetStorage<TOffset> where TBusinessObject : class, IBusinessObject, IEventFeedOffsetContainer<TOffset>, new()
    {
        public BusinessObjectStorageOffsetContainer(
            [NotNull] IBusinessObjectStorage<TBusinessObject> businessObjectStorage
            )
        {
            this.businessObjectStorage = businessObjectStorage;
        }

        public string GetDescription()
        {
            return string.Format("OffsetStorage for {0} in BusinessObject of type {1}", typeof(TOffset).Name, typeof(TBusinessObject).Name);
        }

        public TOffset Read()
        {
            return businessObjectStorage.InScope(key).TryRead(key)?.Offset ?? default(TOffset);
        }

        public void Write(TOffset newOffset)
        {
            businessObjectStorage.InScope(key).Write(new TBusinessObject {ScopeId = key, Id = key, Offset = newOffset});
        }

        private const string key = "EMPTY";
        private readonly IBusinessObjectStorage<TBusinessObject> businessObjectStorage;
    }
}