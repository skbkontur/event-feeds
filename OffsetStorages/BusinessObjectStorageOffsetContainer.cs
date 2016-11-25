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

        public TOffset Read(string key)
        {
            return businessObjectStorage.InScope(CoerceKey(key)).TryRead(CoerceKey(key)).Return(x => x.Offset, default(TOffset));
        }

        private static string CoerceKey(string key)
        {
            return string.IsNullOrWhiteSpace(key) ? "EMPTY" : key;
        }

        public void Write(string key, TOffset offset)
        {
            businessObjectStorage.InScope(CoerceKey(key)).Write(new TBusinessObject {ScopeId = CoerceKey(key), Id = CoerceKey(key), Offset = offset});
        }

        private readonly IBusinessObjectStorage<TBusinessObject> businessObjectStorage;
    }
}