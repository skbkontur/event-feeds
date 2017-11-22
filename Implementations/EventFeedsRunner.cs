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
                                [NotNull] ICatalogueGraphiteClient graphiteClient,
                                [NotNull] IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.graphiteClient = graphiteClient;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            reportActualizationLagJobName = $"{compositeFeedKey ?? nameof(EventFeedsRunner)}-ReportActualizationLagJob";
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
                    EventFeedsRegistry.Register(blade);
                    periodicJobRunnerWithLeaderElection.RunPeriodicJob(FormatFeedJobName(blade), delayBetweenIterations, blade.ExecuteFeeding, blade.Initialize, blade.Shutdown);
                    runningFeeds.Add(blade);
                }
            }
            else
            {
                var eventFeed = new CompositeEventFeed(compositeFeedKey, blades);
                EventFeedsRegistry.Register(eventFeed);
                periodicJobRunnerWithLeaderElection.RunPeriodicJob(FormatFeedJobName(eventFeed), delayBetweenIterations, eventFeed.ExecuteFeeding, eventFeed.Initialize, eventFeed.Shutdown);
                runningFeeds.Add(eventFeed);
            }
            RunningFeeds = runningFeeds.ToArray();

            var actualizationLagGraphitePathPrefix = $"EDI.SubSystem.EventFeeds.ActualizationLag.{Environment.MachineName}";
            periodicJobRunnerWithLeaderElection.RunPeriodicJob(reportActualizationLagJobName, TimeSpan.FromMinutes(1), () =>
                {
                    foreach(var blade in blades)
                        ReportActualizationLagToGraphite(actualizationLagGraphitePathPrefix, blade);
                });
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

        private void ReportActualizationLagToGraphite([NotNull] string graphitePathPrefix, [NotNull] IBlade blade)
        {
            var currentGlobalOffsetTimestamp = blade.GetCurrentGlobalOffsetTimestamp();
            if(currentGlobalOffsetTimestamp != null)
            {
                var now = Timestamp.Now;
                var graphitePath = $"{graphitePathPrefix}.{blade.BladeId.BladeKey}";
                graphiteClient.Send(graphitePath, (long)(now - currentGlobalOffsetTimestamp).TotalMilliseconds, now.ToDateTime());
            }
        }

        private readonly string reportActualizationLagJobName;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
    }
}