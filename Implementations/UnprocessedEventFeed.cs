using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using log4net;
using MoreLinq;
using SKBKontur.Catalogue.CassandraStorageCore.GlobalTicks;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    internal class UnprocessedEventFeed<TEvent> : IEventFeed where TEvent : GenericEvent
    {
        public UnprocessedEventFeed(
            [NotNull] string key, 
            [NotNull] IUnprocessedEventsStorage<TEvent> unprocessedEventsStorage, 
            [NotNull] IEventConsumer<TEvent> consumer,
            [NotNull] IGlobalTicksHolder globalTicksHolder,
            bool leaderElectionRequired)
        {
            this.key = key;
            this.unprocessedEventsStorage = unprocessedEventsStorage;
            this.consumer = consumer;
            this.globalTicksHolder = globalTicksHolder;
            LeaderElectionRequired = leaderElectionRequired;
        }

        public string Key { get { return key; } }

        public bool LeaderElectionRequired { get; private set; }

        public void ExecuteFeeding()
        {
            if (eventFeedStopped)
                ThrowHasEventsWithoutProcessingMarker();

            var feedingStartTicks = globalTicksHolder.GetNowTicks();

            var events = unprocessedEventsStorage.GetEvents();
            logger.InfoFormat("Begin processing unprocessed events (count = {0}).", events.Length);

            events
                .Batch(1000)
                .Pipe(ProcessElementaryEvents)
                .Consume();

            ClearUnprocessedEvents(feedingStartTicks);

            logger.InfoFormat("End processing unprocessed events.");
        }

        public void ExecuteForcedFeeding()
        {
            ExecuteFeeding();
        }

        public bool AreEventsProcessedAt(DateTime dateTime)
        {
            return true;
        }

        public TimeSpan? GetCurrentActualizationLag()
        {
            return null;
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
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
            
            unprocessedEventsStorage.RemoveEvents(objectMutationEvents.Where(x => x.IsProcessed.Value).Select(x => x.Event));
        }

        private static void ThrowHasEventsWithoutProcessingMarker()
        {
            throw new InvalidProgramStateException("Event feed stopped due to forgotten processing marker in one or more events. Consumer did not call MarkAsProcessed or MarkAsUnprocessed methods on IObjectMutationEvent");
        }

        private void ClearUnprocessedEvents(long indexMetaStartGlobalTime)
        {
            var oldEvents = unprocessedEventsStorage.GetEvents().Where(x => (indexMetaStartGlobalTime - x.Ticks) > TimeSpan.FromMinutes(20).Ticks);
            unprocessedEventsStorage.RemoveEvents(oldEvents);
        }

        private readonly string key;
        private readonly IUnprocessedEventsStorage<TEvent> unprocessedEventsStorage;
        private readonly IEventConsumer<TEvent> consumer;
        private readonly IGlobalTicksHolder globalTicksHolder;

        private readonly ILog logger = LogManager.GetLogger(typeof(UnprocessedEventFeed<TEvent>));
        private bool eventFeedStopped;
    }
}