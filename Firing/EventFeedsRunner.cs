using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds.Implementations;
using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeeds.Firing
{
    public class EventFeedsRunner<TEvent, TOffset> : IEventFeedsRunner
    {
        public EventFeedsRunner([NotNull] string key,
                                bool inParallel,
                                TimeSpan delayBetweenIterations,
                                List<DelayedEventFeed<TEvent, TOffset>> blades,
                                ICatalogueGraphiteClient graphiteClient,
                                IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.graphiteClient = graphiteClient;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            reportActualizationLagJobName = string.Format("{0}-ReportActualizationLagJob", key);
            RunFeeds(key, inParallel, delayBetweenIterations, blades);
        }

        [NotNull]
        public List<IEventFeed> RunningFeeds { get; private set; }

        private void RunFeeds([NotNull] string key, bool inParallel, TimeSpan delayBetweenIterations, [NotNull] List<DelayedEventFeed<TEvent, TOffset>> blades)
        {
            if(!blades.Any())
                throw new InvalidProgramStateException("No feeds to run");

            RunningFeeds = new List<IEventFeed>();
            if(!inParallel)
            {
                var eventFeed = new CompositeEventFeed<TEvent, TOffset>(key, blades);
                RunFeed(eventFeed, delayBetweenIterations);
                RunningFeeds.Add(eventFeed);
            }
            else
            {
                foreach(var eventFeed in blades)
                {
                    RunFeed(eventFeed, delayBetweenIterations);
                    RunningFeeds.Add(eventFeed);
                }
            }

            var actualizationLagGraphitePathPrefix = string.Format("EDI.SubSystem.EventFeeds.ActualizationLag.{0}", Environment.MachineName);
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

        public void StopFeeds()
        {
            periodicJobRunnerWithLeaderElection.StopPeriodicJob(reportActualizationLagJobName);
            foreach(var eventFeed in RunningFeeds)
            {
                periodicJobRunnerWithLeaderElection.StopPeriodicJob(FormatFeedJobName(eventFeed));
                EventFeedsRegistry.Unregister(eventFeed.Key);
            }
        }

        [NotNull]
        private static string FormatFeedJobName([NotNull] IEventFeed eventFeed)
        {
            return string.Format("{0}-PeriodicJob", eventFeed.Key);
        }

        private void ReportActualizationLagToGraphite([NotNull] string graphitePathPrefix, [NotNull] DelayedEventFeed<TEvent, TOffset> blade)
        {
            var currentGlobalOffsetTimestamp = blade.GetCurrentGlobalOffsetTimestamp();
            if(currentGlobalOffsetTimestamp != null)
            {
                var now = Timestamp.Now;
                var graphitePath = string.Format("{0}.{1}", graphitePathPrefix, blade.Key);
                graphiteClient.Send(graphitePath, (long)(now - currentGlobalOffsetTimestamp).TotalMilliseconds, now.ToDateTime());
            }
        }

        private readonly string reportActualizationLagJobName;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
    }
}