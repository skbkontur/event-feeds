namespace SKBKontur.Catalogue.Core.EventFeed.Providers.BusinessObjectStorage
{
    public interface IEventFeedOffsetContainer<TOffset>
    {
        TOffset Offset { get; set; }
    }
}