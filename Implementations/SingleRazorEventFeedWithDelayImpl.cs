using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

using JetBrains.Annotations;

using log4net;

using MoreLinq;

using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeed.Interfaces;
using SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl.Interfaces;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Ranges;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl.Implementation
{
    internal class SingleRazorEventFeedWithDelayImpl<TEvent> : IEventFeed where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
    {
        public SingleRazorEventFeedWithDelayImpl(
            [NotNull] string key,
            [NotNull] IGlobalTicksHolder globalTicksHolder,
            [NotNull] IEventLogEventSource<TEvent> eventSource,
            [NotNull] IOffsetStorage<long> offsetStorage,
            [NotNull] IEventConsumer<TEvent> consumer,
            [NotNull] IUnprocessedEventsStorage<TEvent> unprocessedEventsStorage,
            [NotNull] ICatalogueGraphiteClient graphiteClient,
            [NotNull] EventFeedGraphitePaths graphitePaths,
            TimeSpan delay,
            bool leaderElectionRequired)
        {
            this.globalTicksHolder = globalTicksHolder;
            this.eventSource = eventSource;
            this.offsetStorage = offsetStorage;
            this.consumer = consumer;
            this.unprocessedEventsStorage = unprocessedEventsStorage;
            this.graphiteClient = graphiteClient;
            this.graphitePaths = graphitePaths;
            this.delay = delay;
            logger.Info(GetComponentsDescription());
            Key = key;
            LeaderElectionRequired = leaderElectionRequired;
        }

        private long? LocalOffset
        {
            get
            {
                lock(localOffsetLockObject)
                {
                    return localOffset;
                }
            }
            set
            {
                lock(localOffsetLockObject)
                {
                    localOffset = value;
                }
            }
        }

        [NotNull]
        private string GetComponentsDescription()
        {
            var result = new StringBuilder();
            result.AppendLine("Initialized delayed single razor feed with:");
            result.AppendFormat("  EventSource            : {0}", eventSource.GetDescription()).AppendLine();
            result.AppendFormat("  OffsetStorage          : {0}", offsetStorage.GetDescription()).AppendLine();
            result.AppendFormat("  UnprocessedEventStorage: {0}", unprocessedEventsStorage.GetDescription()).AppendLine();
            return result.ToString();
        }

        public void Initialize()
        {
            ResetLocalCaches();
            consumer.Initialize();
        }

        public void Shutdown()
        {
            consumer.Shutdown();
            ResetLocalCaches();
        }

        private void ResetLocalCaches()
        {
            LocalOffset = null;
        }

        public void ExecuteForcedFeeding()
        {
            ExecuteFeedingInternal(false);
        }

        public bool AreEventsProcessedAt(DateTime dateTime)
        {
            return GetCurrentOffset() >= dateTime.Ticks;
        }

        public TimeSpan? GetCurrentActualizationLag()
        {
            var currentOffset = GetCurrentOffset();
            return currentOffset > 0 ? TimeSpan.FromTicks(DateTime.UtcNow.Ticks - currentOffset) : (TimeSpan?)null;
        }

        [NotNull]
        public string Key { get; private set; }

        public bool LeaderElectionRequired { get; private set; }

        public void ExecuteFeeding()
        {
            ExecuteFeedingInternal(true);
        }

        private void ExecuteFeedingInternal(bool useDelay)
        {
            lock(lockObject)
            {
                CheckEventFeedState();

                var feedingStartTicks = globalTicksHolder.GetNowTicks();
                var currentOffset = GetCurrentOffset();

                var range = Range.OfOrEmpty(currentOffset, useDelay ? feedingStartTicks - delay.Ticks : feedingStartTicks);

                if(!range.IsEmpty)
                {
                    var offset = range.LowerBound;
                    while(true)
                    {
                        var eventsBatch = eventSource.GetEvents(offset, range.UpperBound, 1000);
                        eventsBatch.Events.SelectMany(SplitToElementaryEvents).Batch(2000, Enumerable.ToArray).ForEach(ProcessElementaryEvents);
                        unprocessedEventsStorage.Flush();
                        SetLastEventInfo(eventsBatch.LastOffset);
                        offset = eventsBatch.LastOffset;
                        if(eventsBatch.NoMoreEventsInSource)
                            break;
                    }
                }
                unprocessedEventsStorage.Flush();
                SetLastEventInfo(useDelay ? (feedingStartTicks - delay.Ticks) : feedingStartTicks);
                SendStatsToGraphite();
                logger.InfoFormat("End processing events");
            }
        }

        private void CheckEventFeedState()
        {
            if(eventFeedStopped)
                ThrowHasEventsWithoutProcessingMarker();
        }

        private static void ThrowHasEventsWithoutProcessingMarker()
        {
            throw new InvalidProgramStateException("Event feed stopped due to forgotten processing marker in one or more events. Consumer did not call MarkAsProcessed or MarkAsUnprocessed methods on IObjectMutationEvent");
        }

        private long GetCurrentOffset()
        {
            return LocalOffset ?? offsetStorage.Read(null);
        }

        private void SetLastEventInfo(long lastEventInfo)
        {
            LocalOffset = Math.Max(LocalOffset ?? 0, lastEventInfo);
            var offsetInStorage = offsetStorage.Read(null);
            logger.InfoFormat("SetLastEventInfo: {0}, LocalOffset: {1}, OffsetStorage: {2}", FormatOffset(lastEventInfo), FormatOffset(LocalOffset), FormatOffset(offsetInStorage));
            offsetStorage.Write(null, Math.Max(offsetInStorage, lastEventInfo));
        }

        private static string FormatOffset(long? currentOffset)
        {
            if(!currentOffset.HasValue)
                return "NULL";
            var minDate = new DateTime(1990, 01, 01);
            if(currentOffset < minDate.Ticks)
                return currentOffset.ToString();
            return new DateTime(currentOffset.Value).ToString(CultureInfo.InvariantCulture) + " (" + currentOffset + ")";
        }

        private void ProcessElementaryEvents([NotNull] IEnumerable<TEvent> elementaryWriteEvents)
        {
            var objectMutationEvents = elementaryWriteEvents.Select(x => new ObjectMutationEvent<TEvent>
                {
                    Event = x
                }).ToArray();

            consumer.ProcessEvents(objectMutationEvents.Cast<IObjectMutationEvent<TEvent>>().ToArray());

            if(objectMutationEvents.Any(x => !x.IsProcessed.HasValue))
            {
                eventFeedStopped = true;
                ThrowHasEventsWithoutProcessingMarker();
            }

            unprocessedEventsStorage.AddEvents(objectMutationEvents.Where(x => !x.IsProcessed.Value).Select(x => x.Event));
            unprocessedEventsStorage.RemoveEvents(objectMutationEvents.Where(x => x.IsProcessed.Value).Select(x => x.Event));
        }

        [NotNull]
        private static IEnumerable<TEvent> SplitToElementaryEvents([NotNull] TEvent eventObject)
        {
            return eventObject.SplitToElementary();
        }

        private void SendStatsToGraphite()
        {
            if(lastSendToGraphiteTime.HasValue && DateTime.UtcNow < lastSendToGraphiteTime.Value + TimeSpan.FromMinutes(1))
                return;
            var lag = GetCurrentActualizationLag();
            if (lag.HasValue && graphitePaths.ActualizationLag != null)
                graphiteClient.Send(graphitePaths.ActualizationLag, (long) lag.Value.TotalMilliseconds, DateTime.UtcNow);
            lastSendToGraphiteTime = DateTime.UtcNow;
        }

        private DateTime? lastSendToGraphiteTime;
        private readonly object localOffsetLockObject = new object();
        private long? localOffset;
        private bool eventFeedStopped;

        private readonly object lockObject = new object();
        private readonly ILog logger = LogManager.GetLogger(typeof(SingleRazorEventFeedWithDelayImpl<TEvent>));
        private readonly IGlobalTicksHolder globalTicksHolder;
        private readonly IEventLogEventSource<TEvent> eventSource;
        private readonly IOffsetStorage<long> offsetStorage;
        private readonly IEventConsumer<TEvent> consumer;
        private readonly IUnprocessedEventsStorage<TEvent> unprocessedEventsStorage;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly EventFeedGraphitePaths graphitePaths;
        private readonly TimeSpan delay;
    }
}