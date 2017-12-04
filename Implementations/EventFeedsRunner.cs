using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class EventFeedsRunner : IEventFeedsRunner
    {
        public EventFeedsRunner([CanBeNull] string compositeFeedKey,
                                TimeSpan delayBetweenIterations,
                                [NotNull, ItemNotNull] IBlade[] blades,
                                ICatalogueGraphiteClient graphiteClient,
                                IPeriodicTaskRunner periodicTaskRunner,
                                IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.graphiteClient = graphiteClient;
            this.periodicTaskRunner = periodicTaskRunner;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            RunFeeds(compositeFeedKey, delayBetweenIterations, blades);
        }

        [NotNull]
        public IEventFeed[] RunningFeeds { get; private set; }

        private void RunFeeds([CanBeNull] string compositeFeedKey, TimeSpan delayBetweenIterations, [NotNull, ItemNotNull] IBlade[] blades)
        {
            if(!blades.Any())
                throw new InvalidProgramStateException("No feeds to run");

            var runningFeeds = new List<IEventFeed>();
            if(string.IsNullOrEmpty(compositeFeedKey))
            {
                foreach(var blade in blades)
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
            RunningFeeds = runningFeeds.ToArray();
        }

        private void RunFeed([NotNull] EventFeed eventFeed, TimeSpan delayBetweenIterations)
        {
            periodicJobRunnerWithLeaderElection.RunPeriodicJob(FormatFeedJobName(eventFeed), delayBetweenIterations, eventFeed.ExecuteFeeding, eventFeed.Initialize, eventFeed.Shutdown);
        }

        public void Stop()
        {
            foreach(var eventFeed in RunningFeeds)
                periodicJobRunnerWithLeaderElection.StopPeriodicJob(FormatFeedJobName(eventFeed));
        }

        [NotNull]
        private static string FormatFeedJobName([NotNull] IEventFeed eventFeed)
        {
            return $"{eventFeed.FeedKey}-PeriodicJob";
        }

        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
    }
}