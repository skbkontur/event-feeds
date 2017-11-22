using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class EventFeedsRunner<TEvent, TOffset> : IEventFeedsRunner
    {
        public EventFeedsRunner([NotNull] string key,
                                bool inParallel,
                                TimeSpan delayBetweenIterations,
                                [NotNull, ItemNotNull] Blade<TEvent, TOffset>[] blades,
                                [NotNull] ICatalogueGraphiteClient graphiteClient,
                                [NotNull] IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.graphiteClient = graphiteClient;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            reportActualizationLagJobName = $"{key}-ReportActualizationLagJob";
            RunFeeds(key, inParallel, delayBetweenIterations, blades);
        }

        [NotNull]
        public IEventFeed[] RunningFeeds { get; private set; }

        private void RunFeeds([NotNull] string key, bool inParallel, TimeSpan delayBetweenIterations, [NotNull, ItemNotNull] Blade<TEvent, TOffset>[] blades)
        {
            if(!blades.Any())
                throw new InvalidProgramStateException("No feeds to run");

            var runningFeeds = new List<IEventFeed>();
            if(!inParallel)
            {
                var eventFeed = new CompositeEventFeed(key, blades.Cast<IEventFeed>().ToArray());
                RunFeed(eventFeed, delayBetweenIterations);
                runningFeeds.Add(eventFeed);
            }
            else
            {
                foreach(var eventFeed in blades)
                {
                    RunFeed(eventFeed, delayBetweenIterations);
                    runningFeeds.Add(eventFeed);
                }
            }
            RunningFeeds = runningFeeds.ToArray();

            var actualizationLagGraphitePathPrefix = $"EDI.SubSystem.EventFeeds.ActualizationLag.{Environment.MachineName}";
            periodicJobRunnerWithLeaderElection.RunPeriodicJob(reportActualizationLagJobName, TimeSpan.FromMinutes(1), () =>
                {
                    foreach(var blade in blades)
                        ReportActualizationLagToGraphite(actualizationLagGraphitePathPrefix, blade);
                });
        }

        private void RunFeed([NotNull] IEventFeed eventFeed, TimeSpan delayBetweenIterations)
        {
            EventFeedsRegistry.Register(eventFeed);
            periodicJobRunnerWithLeaderElection.RunPeriodicJob(FormatFeedJobName(eventFeed), delayBetweenIterations, eventFeed.ExecuteFeeding, eventFeed.Initialize, eventFeed.Shutdown);
        }

        public void Stop()
        {
            periodicJobRunnerWithLeaderElection.StopPeriodicJob(reportActualizationLagJobName);
            foreach(var eventFeed in RunningFeeds)
            {
                periodicJobRunnerWithLeaderElection.StopPeriodicJob(FormatFeedJobName(eventFeed));
                EventFeedsRegistry.Unregister(eventFeed.FeedKey);
            }
        }

        [NotNull]
        private static string FormatFeedJobName([NotNull] IEventFeed eventFeed)
        {
            return $"{eventFeed.FeedKey}-PeriodicJob";
        }

        private void ReportActualizationLagToGraphite([NotNull] string graphitePathPrefix, [NotNull] Blade<TEvent, TOffset> blade)
        {
            var currentGlobalOffsetTimestamp = blade.GetCurrentGlobalOffsetTimestamp();
            if(currentGlobalOffsetTimestamp != null)
            {
                var now = Timestamp.Now;
                var graphitePath = $"{graphitePathPrefix}.{blade.FeedKey}";
                graphiteClient.Send(graphitePath, (long)(now - currentGlobalOffsetTimestamp).TotalMilliseconds, now.ToDateTime());
            }
        }

        private readonly string reportActualizationLagJobName;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
    }
}