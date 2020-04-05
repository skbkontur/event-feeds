using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.EventFeeds.Implementations
{
    public class EventFeedsRunner : IEventFeedsRunner
    {
        public EventFeedsRunner([CanBeNull] string compositeFeedKey,
                                TimeSpan delayBetweenIterations,
                                [NotNull, ItemNotNull] IBlade[] blades,
                                IPeriodicJobRunner periodicJobRunner)
        {
            this.blades = blades;
            this.periodicJobRunner = periodicJobRunner;
            RunFeeds(compositeFeedKey, delayBetweenIterations);
        }

        private void RunFeeds([CanBeNull] string compositeFeedKey, TimeSpan delayBetweenIterations)
        {
            if (!blades.Any())
                throw new InvalidOperationException("No feeds to run");

            if (string.IsNullOrEmpty(compositeFeedKey))
            {
                foreach (var blade in blades)
                {
                    var eventFeed = new EventFeed(blade);
                    RunFeed(eventFeed, delayBetweenIterations);
                    runningFeeds.Add(eventFeed);
                }
            }
            else
            {
                var eventFeed = new EventFeed(compositeFeedKey, blades);
                RunFeed(eventFeed, delayBetweenIterations);
                runningFeeds.Add(eventFeed);
            }
        }

        private void RunFeed([NotNull] EventFeed eventFeed, TimeSpan delayBetweenIterations)
        {
            periodicJobRunner.RunPeriodicJobWithLeaderElection(FormatFeedJobName(eventFeed),
                                                               delayBetweenIterations,
                                                               jobAction : () => ExecuteFeeding(eventFeed),
                                                               onTakeTheLead : eventFeed.Initialize,
                                                               onLoseTheLead : eventFeed.Shutdown);
        }

        private static void ExecuteFeeding([NotNull] EventFeed eventFeed)
        {
            lock (eventFeed)
                eventFeed.ExecuteFeeding();
        }

        private static void ExecuteForcedFeeding([NotNull] EventFeed eventFeed, TimeSpan delayUpperBound)
        {
            try
            {
                lock (eventFeed)
                    eventFeed.ExecuteForcedFeeding(delayUpperBound);
            }
            catch (Exception)
            {
                eventFeed.Shutdown();
                throw;
            }
        }

        public ( /*[NotNull]*/ BladeId BladeId, /*[CanBeNull]*/ Timestamp CurrentGlobalOffsetTimestamp)[] GetCurrentGlobalOffsetTimestamps()
        {
            return blades.Select(x => (x.BladeId, CurrentGlobalOffsetTimestamp : x.GetCurrentGlobalOffsetTimestamp())).ToArray();
        }

        public void ResetLocalState()
        {
            foreach (var eventFeed in runningFeeds)
                eventFeed.ResetLocalState();
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            foreach (var eventFeed in runningFeeds)
                ExecuteForcedFeeding(eventFeed, delayUpperBound);
        }

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            return runningFeeds.All(eventFeed => eventFeed.AreEventsProcessedAt(timestamp));
        }

        public void Stop()
        {
            foreach (var eventFeed in runningFeeds)
                periodicJobRunner.StopPeriodicJobWithLeaderElection(FormatFeedJobName(eventFeed));
        }

        [NotNull]
        private static string FormatFeedJobName([NotNull] EventFeed eventFeed)
        {
            return $"{eventFeed.FeedKey}-PeriodicJob";
        }

        private readonly IBlade[] blades;
        private readonly IPeriodicJobRunner periodicJobRunner;
        private readonly List<EventFeed> runningFeeds = new List<EventFeed>();
    }
}