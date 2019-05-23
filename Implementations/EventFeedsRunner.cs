using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

using SkbKontur.Graphite.Client;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class EventFeedsRunner : IEventFeedsRunner
    {
        public EventFeedsRunner([CanBeNull] string compositeFeedKey,
                                TimeSpan delayBetweenIterations,
                                [NotNull, ItemNotNull] IBlade[] blades,
                                IGraphiteClient graphiteClient,
                                IPeriodicTaskRunner periodicTaskRunner,
                                IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.graphiteClient = graphiteClient;
            this.periodicTaskRunner = periodicTaskRunner;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            RunFeeds(compositeFeedKey, delayBetweenIterations, blades);
        }

        private void RunFeeds([CanBeNull] string compositeFeedKey, TimeSpan delayBetweenIterations, [NotNull, ItemNotNull] IBlade[] blades)
        {
            if (!blades.Any())
                throw new InvalidProgramStateException("No feeds to run");

            if (string.IsNullOrEmpty(compositeFeedKey))
            {
                foreach (var blade in blades)
                {
                    var eventFeed = new EventFeed(blade, graphiteClient, periodicTaskRunner);
                    RunFeed(eventFeed, delayBetweenIterations);
                    runningFeeds.Add(eventFeed);
                }
            }
            else
            {
                var eventFeed = new EventFeed(compositeFeedKey, blades, graphiteClient, periodicTaskRunner);
                RunFeed(eventFeed, delayBetweenIterations);
                runningFeeds.Add(eventFeed);
            }
        }

        private void RunFeed([NotNull] EventFeed eventFeed, TimeSpan delayBetweenIterations)
        {
            periodicJobRunnerWithLeaderElection.RunPeriodicJob(FormatFeedJobName(eventFeed), delayBetweenIterations, () => ExecuteFeeding(eventFeed), eventFeed.Initialize, eventFeed.Shutdown);
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
                periodicJobRunnerWithLeaderElection.StopPeriodicJob(FormatFeedJobName(eventFeed));
        }

        [NotNull]
        private static string FormatFeedJobName([NotNull] EventFeed eventFeed)
        {
            return $"{eventFeed.FeedKey}-PeriodicJob";
        }

        private readonly IGraphiteClient graphiteClient;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
        private readonly List<EventFeed> runningFeeds = new List<EventFeed>();
    }
}