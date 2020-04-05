using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

using Vostok.Logging.Abstractions;

namespace SkbKontur.EventFeeds.Implementations
{
    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    public class Blade<TEvent, TOffset> : IBlade
    {
        public Blade(BladeId bladeId,
                     IGlobalTimeProvider globalTimeProvider,
                     IEventSource<TEvent, TOffset> eventSource,
                     IOffsetStorage<TOffset> offsetStorage,
                     IOffsetInterpreter<TOffset> offsetInterpreter,
                     IEventConsumer<TEvent, TOffset> eventConsumer,
                     ILog logger)
        {
            BladeId = bladeId;
            this.globalTimeProvider = globalTimeProvider;
            this.eventSource = eventSource;
            this.offsetStorage = offsetStorage;
            this.offsetInterpreter = offsetInterpreter;
            this.eventConsumer = eventConsumer;
            this.logger = logger.ForContext("DelayedEventFeed");
            LogComponentsDescription();
            offsetHolder = new OffsetHolder(offsetStorage, offsetInterpreter, this.logger);
        }

        private void LogComponentsDescription()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Initialized blade with:");
            sb.Append($"  BladeId      : {BladeId}").AppendLine();
            sb.Append($"  EventSource  : {eventSource.GetDescription()}").AppendLine();
            sb.Append($"  EventConsumer: {eventConsumer.GetDescription()}").AppendLine();
            sb.Append($"  OffsetStorage: {offsetStorage.GetDescription()}").AppendLine();
            logger.Info(sb.ToString());
        }

        [NotNull]
        public BladeId BladeId { get; }

        public void Initialize()
        {
            ResetLocalState();
            feedIsRunning = true;
        }

        public void Shutdown()
        {
            feedIsRunning = false;
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
            DoExecuteFeeding(useDelay : true);
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            if (delayUpperBound < BladeId.Delay)
                return;
            DoExecuteFeeding(useDelay : false);
        }

        private void DoExecuteFeeding(bool useDelay)
        {
            if (!feedIsRunning)
                throw new InvalidOperationException($"Blade with id '{BladeId}' is not running");
            var fromOffsetExclusive = offsetHolder.GetLocalOffset();
            var globalNowTimestamp = globalTimeProvider.GetNowTimestamp();
            var toOffsetInclusive = offsetInterpreter.GetMaxOffsetForTimestamp(useDelay ? globalNowTimestamp - BladeId.Delay : globalNowTimestamp);
            if (offsetInterpreter.Compare(fromOffsetExclusive, toOffsetInclusive) >= 0)
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
                if (eventsProcessingResult.CommitOffset)
                    offsetHolder.UpdateLocalOffset(eventsProcessingResult.GetOffsetToCommit());
                fromOffsetExclusive = eventsQueryResult.LastOffset;
                eventsProcessed += eventsQueryResult.Events.Count;
            } while (!eventsQueryResult.NoMoreEventsInSource);
            logger.Info($"End processing events by blade {BladeId}. Processed {eventsProcessed} events in {sw.Elapsed}");
        }

        private string FormatOffset(TOffset offset)
        {
            return offsetInterpreter.Format(offset);
        }

        private bool feedIsRunning;
        private readonly ILog logger;
        private readonly IGlobalTimeProvider globalTimeProvider;
        private readonly IEventSource<TEvent, TOffset> eventSource;
        private readonly IOffsetStorage<TOffset> offsetStorage;
        private readonly IOffsetInterpreter<TOffset> offsetInterpreter;
        private readonly IEventConsumer<TEvent, TOffset> eventConsumer;
        private readonly OffsetHolder offsetHolder;

        private class OffsetHolder
        {
            public OffsetHolder(IOffsetStorage<TOffset> offsetStorage, IOffsetInterpreter<TOffset> offsetInterpreter, ILog logger)
            {
                this.offsetStorage = offsetStorage;
                this.offsetInterpreter = offsetInterpreter;
                this.logger = logger;
            }

            public void Reset()
            {
                lock (locker)
                {
                    localOffset = default;
                    localOffsetWasSet = false;
                }
            }

            [CanBeNull]
            public TOffset GetLocalOffset()
            {
                lock (locker)
                    return localOffsetWasSet ? localOffset : offsetStorage.Read();
            }

            public void UpdateLocalOffset([NotNull] TOffset newOffset)
            {
                lock (locker)
                {
                    localOffset = offsetInterpreter.Max(localOffset, newOffset);
                    localOffsetWasSet = true;
                    var offsetInStorage = offsetStorage.Read();
                    logger.Info($"NewOffset: {FormatOffset(newOffset)}, LocalOffset: {FormatOffset(localOffset)}, OffsetInStorage: {FormatOffset(offsetInStorage)}");
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
            private readonly ILog logger;
        }
    }
}