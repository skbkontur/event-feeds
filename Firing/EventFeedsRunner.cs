using System;
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
                                DelayedEventFeed<TEvent, TOffset>[] blades,
                                ICatalogueGraphiteClient graphiteClient,
                                IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection)
        {
            this.graphiteClient = graphiteClient;
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            reportActualizationLagJobName = $"{key}-ReportActualizationLagJob";
            RunFeeds(key, inParallel, delayBetweenIterations, blades);
        }

        [NotNull]
        public ICompositeEventFeed RunningFeed { get; private set; }

        private void RunFeeds([NotNull] string key, bool inParallel, TimeSpan delayBetweenIterations, [NotNull] DelayedEventFeed<TEvent, TOffset>[] blades)
        {
            if(!blades.Any())
                throw new InvalidProgramStateException("No feeds to run");

            if(inParallel)
            {
                RunningFeed = new ParallelEventFeed<TEvent, TOffset>(periodicJobRunnerWithLeaderElection, key, blades);
            }
            else
            {
                RunningFeed = new ConsequentEventFeed<TEvent, TOffset>(periodicJobRunnerWithLeaderElection, key, blades);
            }
            RunningFeed.RunFeed(delayBetweenIterations);


            var actualizationLagGraphitePathPrefix = string.Format("EDI.SubSystem.EventFeeds.ActualizationLag.{0}", Environment.MachineName);
            periodicJobRunnerWithLeaderElection.RunPeriodicJob(reportActualizationLagJobName, TimeSpan.FromMinutes(1), () =>
                {
                    foreach(var blade in blades)
                        ReportActualizationLagToGraphite(actualizationLagGraphitePathPrefix, blade);
                });
        }

        public void StopFeed()
        {
            periodicJobRunnerWithLeaderElection.StopPeriodicJob(reportActualizationLagJobName);
            RunningFeed.StopFeed();
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