using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;

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
            return $"OffsetStorage for {typeof(TOffset).Name} in BusinessObject of type {typeof(TBusinessObject).Name}";
        }

        public TOffset Read()
        {
            var businessObject = businessObjectStorage.InScope(key).TryRead(key);
            if (businessObject != null)
                return businessObject.Offset;
            return default(TOffset);
        }

        public void Write(TOffset newOffset)
        {
            businessObjectStorage.InScope(key).Write(new TBusinessObject {ScopeId = key, Id = key, Offset = newOffset});
        }

        private const string key = "EMPTY";
        private readonly IBusinessObjectStorage<TBusinessObject> businessObjectStorage;
    }
}