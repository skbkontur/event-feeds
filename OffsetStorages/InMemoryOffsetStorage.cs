namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    public class InMemoryOffsetStorage<TOffset> : IOffsetStorage<TOffset>
    {
        public string GetDescription()
        {
            return string.Format("In memory generic offset storage. Offset type: {0}", typeof(TOffset).Name);
        }

        public TOffset Read()
        {
            return offset;
        }

        public void Write(TOffset newOffset)
        {
            offset = newOffset;
        }

        private TOffset offset;
    }
}