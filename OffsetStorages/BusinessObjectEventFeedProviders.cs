using GroboContainer.Core;
using JetBrains.Annotations;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    public class BusinessObjectEventFeedProviders
    {
        public BusinessObjectEventFeedProviders(
            [NotNull] IContainer container
            )
        {
            this.container = container;
        }

        [NotNull]
        public OffsetStorageConfigurationHelper<TOffset> OffsetStorage<TOffset>()
        {
            return new OffsetStorageConfigurationHelper<TOffset>(container);
        }

        private readonly IContainer container;

        public class OffsetStorageConfigurationHelper<TOffset>
        {
            public OffsetStorageConfigurationHelper([NotNull] IContainer container)
            {
                this.container = container;
            }

            public IOffsetStorage<TOffset> InObject<TBusinessObject>() where TBusinessObject : class, IEventFeedOffsetContainer<TOffset>, IBusinessObject, new()
            {
                return new BusinessObjectStorageOffsetContainer<TOffset, TBusinessObject>(container.Get<IBusinessObjectStorage<TBusinessObject>>());
            }

            private readonly IContainer container;
        }
    }
}