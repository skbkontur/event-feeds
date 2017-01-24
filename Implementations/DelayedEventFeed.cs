using System;
using System.Diagnostics;
using System.Text;

using JetBrains.Annotations;

using log4net;

using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.EventFeeds.Building;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.Comparing;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class DelayedEventFeed<TEvent, TOffset> : IEventFeed
    {
        public DelayedEventFeed(BladeId bladeId,
                                IGlobalTicksHolder globalTicksHolder,
                                IEventSource<TEvent, TOffset> eventSource,
                                IOffsetStorage<TOffset> offsetStorage,
                                IOffsetInterpreter<TOffset> offsetInterpreter,
                                IEventConsumer<TEvent, TOffset> eventConsumer)
        {
            this.bladeId = bladeId;
            this.globalTicksHolder = globalTicksHolder;
            this.eventSource = eventSource;
            this.offsetStorage = offsetStorage;
            this.offsetInterpreter = offsetInterpreter;
            this.eventConsumer = eventConsumer;
            LogComponentsDescription();
            offsetHolder = new OffsetHolder(offsetStorage, offsetInterpreter);
        }

        private void LogComponentsDescription()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Initialized delayed single razor feed with:");
            sb.AppendFormat("  EventSource  : {0}", eventSource.GetDescription()).AppendLine();
            sb.AppendFormat("  EventConsumer: {0}", eventConsumer.GetDescription()).AppendLine();
            sb.AppendFormat("  OffsetStorage: {0}", offsetStorage.GetDescription()).AppendLine();
            logger.Info(sb.ToString());
        }

        [NotNull]
        public string Key { get { return bladeId.Key; } }

        public TimeSpan Delay { get { return bladeId.Delay; } }

        public void ResetLocalState()
        {
            offsetHolder.Reset();
            eventConsumer.ResetLocalState();
        }

        [CanBeNull]
        public Timestamp GetCurrentGlobalOffsetTimestamp()
        {
            return offsetInterpreter.GetTimestampFromOffset(offsetStorage.Read());
        }

        public void ExecuteFeeding()
        {
            ExecuteFeedingInternal(useDelay : true);
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            if(delayUpperBound < bladeId.Delay)
                throw new InvalidProgramStateException(string.Format("It is not allowed to force feeding for delay {0} which is smaller than bladeId.Delay ({1})", delayUpperBound, bladeId.Delay));
            ExecuteFeedingInternal(useDelay : false);
        }

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            var localOffset = offsetHolder.GetLocalOffset();
            return offsetInterpreter.GetTimestampFromOffset(localOffset) >= timestamp;
        }

        private void ExecuteFeedingInternal(bool useDelay)
        {
            lock(locker)
            {
                var localOffset = offsetHolder.GetLocalOffset();
                var globalNowTimestamp = new Timestamp(globalTicksHolder.GetNowTicks());
                var toOffsetInclusive = offsetInterpreter.GetMaxOffsetForTimestamp(useDelay ? globalNowTimestamp - bladeId.Delay : globalNowTimestamp);
                if(offsetInterpreter.Compare(toOffsetInclusive, localOffset) <= 0)
                {
                    logger.InfoFormat("Skip processing events by blade {0} because toOffsetInclusive ({1}) <= localOffset ({2}) ", bladeId, toOffsetInclusive, localOffset);
                    return;
                }
                logger.InfoFormat("Start processing events by blade {0}. GlobalNowTimestamp = {1}, LocalOffset = {2}, ToOffsetInclusive = {3}", bladeId, globalNowTimestamp, localOffset, toOffsetInclusive);
                var sw = Stopwatch.StartNew();
                var eventsProcessed = 0;
                var fromOffsetExclusive = localOffset;
                EventsQueryResult<TEvent, TOffset> eventsQueryResult;
                do
                {
                    eventsQueryResult = eventSource.GetEvents(fromOffsetExclusive, toOffsetInclusive, estimatedCount : 5000);
                    var eventsProcessingResult = eventConsumer.ProcessEvents(eventsQueryResult);
                    if(eventsProcessingResult.CommitOffset)
                        offsetHolder.UpdateLocalOffset(eventsProcessingResult.GetOffsetToCommit());
                    fromOffsetExclusive = eventsQueryResult.LastOffset;
                    eventsProcessed += eventsQueryResult.Events.Count;
                } while(!eventsQueryResult.NoMoreEventsInSource);
                logger.InfoFormat("End processing events by blade {0}. Processed {1} events in {2}", bladeId, eventsProcessed, sw.Elapsed);
            }
        }

        private readonly object locker = new object();
        private readonly ILog logger = Log.For("DelayedEventFeed");
        private readonly BladeId bladeId;
        private readonly IGlobalTicksHolder globalTicksHolder;
        private readonly IEventSource<TEvent, TOffset> eventSource;
        private readonly IOffsetStorage<TOffset> offsetStorage;
        private readonly IOffsetInterpreter<TOffset> offsetInterpreter;
        private readonly IEventConsumer<TEvent, TOffset> eventConsumer;
        private readonly OffsetHolder offsetHolder;

        private class OffsetHolder
        {
            public OffsetHolder(IOffsetStorage<TOffset> offsetStorage, IOffsetInterpreter<TOffset> offsetInterpreter)
            {
                this.offsetStorage = offsetStorage;
                this.offsetInterpreter = offsetInterpreter;
            }

            public void Reset()
            {
                lock(locker)
                {
                    localOffset = default(TOffset);
                    localOffsetWasSet = false;
                }
            }

            [CanBeNull]
            public TOffset GetLocalOffset()
            {
                lock(locker)
                    return localOffsetWasSet ? localOffset : offsetStorage.Read();
            }

            public void UpdateLocalOffset([NotNull] TOffset newOffset)
            {
                lock(locker)
                {
                    localOffset = offsetInterpreter.Max(localOffset, newOffset);
                    localOffsetWasSet = true;
                    var offsetInStorage = offsetStorage.Read();
                    Log.For("DelayedEventFeed").InfoFormat("NewOffset: {0}, LocalOffset: {1}, OffsetInStorage: {2}", FormatOffset(newOffset), FormatOffset(localOffset), FormatOffset(offsetInStorage));
                    offsetStorage.Write(offsetInterpreter.Max(localOffset, offsetInStorage));
                }
            }

            private string FormatOffset(TOffset offset)
            {
                return offsetInterpreter.Format(offset);
            }

            private TOffset localOffset;
            private bool localOffsetWasSet;
            private readonly object locker = new object();
            private readonly IOffsetStorage<TOffset> offsetStorage;
            private readonly IOffsetInterpreter<TOffset> offsetInterpreter;
        }
    }
}