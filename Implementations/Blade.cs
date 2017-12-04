using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

using JetBrains.Annotations;

using log4net;

using SKBKontur.Catalogue.Core.EventFeeds.Building;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.Comparing;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    public class Blade<TEvent, TOffset> : IBlade
    {
        public Blade(BladeId bladeId,
                     IGlobalTimeProvider globalTimeProvider,
                     IEventSource<TEvent, TOffset> eventSource,
                     IOffsetStorage<TOffset> offsetStorage,
                     IOffsetInterpreter<TOffset> offsetInterpreter,
                     IEventConsumer<TEvent, TOffset> eventConsumer)
        {
            BladeId = bladeId;
            this.globalTimeProvider = globalTimeProvider;
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
            sb.AppendLine("Initialized blade with:");
            sb.AppendFormat("  BladeId      : {0}", BladeId).AppendLine();
            sb.AppendFormat("  EventSource  : {0}", eventSource.GetDescription()).AppendLine();
            sb.AppendFormat("  EventConsumer: {0}", eventConsumer.GetDescription()).AppendLine();
            sb.AppendFormat("  OffsetStorage: {0}", offsetStorage.GetDescription()).AppendLine();
            logger.Info(sb.ToString());
        }

        [NotNull]
        public BladeId BladeId { get; }

        public void Initialize()
        {
            ResetLocalState();
            feedIsRunningSignal.Set();
        }

        public void Shutdown()
        {
            feedIsRunningSignal.Reset();
            ResetLocalState();
        }

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

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            var localOffset = offsetHolder.GetLocalOffset();
            return offsetInterpreter.GetTimestampFromOffset(localOffset) >= timestamp;
        }

        public void ExecuteFeeding()
        {
            lock(locker)
                DoExecuteFeeding(useDelay : true);
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            if(delayUpperBound < BladeId.Delay)
                return;
            lock(locker)
            {
                feedIsRunningSignal.Wait();
                try
                {
                    DoExecuteFeeding(useDelay : false);
                }
                catch(Exception)
                {
                    Shutdown();
                    throw;
                }
            }
        }

        private void DoExecuteFeeding(bool useDelay)
        {
            var fromOffsetExclusive = offsetHolder.GetLocalOffset();
            var globalNowTimestamp = globalTimeProvider.GetNowTimestamp();
            var toOffsetInclusive = offsetInterpreter.GetMaxOffsetForTimestamp(useDelay ? globalNowTimestamp - BladeId.Delay : globalNowTimestamp);
            if(offsetInterpreter.Compare(fromOffsetExclusive, toOffsetInclusive) >= 0)
            {
                logger.Info($"Skip processing events by blade {BladeId} because fromOffsetExclusive({FormatOffset(fromOffsetExclusive)}) >= toOffsetInclusive ({FormatOffset(toOffsetInclusive)}) ");
                return;
            }
            logger.Info($"Start processing events by blade {BladeId}. GlobalNowTimestamp = {globalNowTimestamp}, FromOffsetExclusive = {FormatOffset(fromOffsetExclusive)}, ToOffsetInclusive = {FormatOffset(toOffsetInclusive)}");
            var eventsProcessed = 0;
            var sw = Stopwatch.StartNew();
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
            logger.Info($"End processing events by blade {BladeId}. Processed {eventsProcessed} events in {sw.Elapsed}");
        }

        private string FormatOffset(TOffset offset)
        {
            return offsetInterpreter.Format(offset);
        }

        private readonly object locker = new object();
        private readonly ManualResetEventSlim feedIsRunningSignal = new ManualResetEventSlim(initialState : false);
        private readonly ILog logger = Log.For("DelayedEventFeed");
        private readonly IGlobalTimeProvider globalTimeProvider;
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
                    Log.For("DelayedEventFeed").Info($"NewOffset: {FormatOffset(newOffset)}, LocalOffset: {FormatOffset(localOffset)}, OffsetInStorage: {FormatOffset(offsetInStorage)}");
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