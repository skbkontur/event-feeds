namespace SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages
{
    public interface IEventFeedOffsetContainer<TOffset>
    {
        TOffset Offset { get; set; }
    }
}