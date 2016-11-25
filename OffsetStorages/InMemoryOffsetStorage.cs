using System.Collections.Concurrent;

using SKBKontur.Catalogue.Core.EventFeed.Interfaces;

namespace SKBKontur.Catalogue.Core.EventFeed.Providers.Implementation
{
    public class InMemoryOffsetStorage<TOffset> : IOffsetStorage<TOffset>
    {
        public string GetDescription()
        {
            return string.Format("In memory generic offset storage. Offset type: {0}", typeof(TOffset).Name);
        }

        public TOffset Read(string key)
        {
            if(key == null)
                return defaultOffset;
            TOffset result;
            return lastEventInfo.TryGetValue(key, out result) ? result : default(TOffset);
        }

        public void Write(string key, TOffset newlastEventInfo)
        {
            if(key == null)
            {
                defaultOffset = newlastEventInfo;
                return;
            }
            lastEventInfo.AddOrUpdate(key, x => newlastEventInfo, (x, y) => newlastEventInfo);
        }

        private TOffset defaultOffset;
        private readonly ConcurrentDictionary<string, TOffset> lastEventInfo = new ConcurrentDictionary<string, TOffset>();
    }
}