using System;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.Graphite.Client;

namespace SkbKontur.EventFeeds.Implementations
{
    public class EventFeed
    {
        public EventFeed([NotNull] IBlade blade, IGraphiteClient graphiteClient, IPeriodicJobRunner periodicJobRunner)
            : this(blade.BladeId.BladeKey, new[] {blade}, graphiteClient, periodicJobRunner)
        {
        }

        public EventFeed([NotNull] string feedKey,
                         [NotNull, ItemNotNull] IBlade[] blades,
                         IGraphiteClient graphiteClient,
                         IPeriodicJobRunner periodicJobRunner)
        {
            FeedKey = feedKey;
            this.blades = blades;
            this.graphiteClient = graphiteClient;
            this.periodicJobRunner = periodicJobRunner;
            graphitePathPrefix = $"SubSystem.EventFeeds.ActualizationLag.{Environment.MachineName}";
        }

        [NotNull]
        public string FeedKey { get; }

        [NotNull]
        private string LagReportingJobName => $"{FeedKey}-ReportActualizationLagJob";

        public void Initialize()
        {
            foreach (var blade in blades)
                blade.Initialize();

            periodicJobRunner.RunPeriodicJob(LagReportingJobName,
                                             delayBetweenIterations : TimeSpan.FromMinutes(1),
                                             jobAction : ReportActualizationLagToGraphite);
        }

        private void ReportActualizationLagToGraphite()
        {
            var offsetsToReport = blades.Select(x => (bladeKey : x.BladeId.BladeKey, currentGlobalOffsetTimestamp : x.GetCurrentGlobalOffsetTimestamp()))
                                        .Where(t => t.currentGlobalOffsetTimestamp != null)
                                        .ToArray();
            var now = Timestamp.Now;
            foreach (var t in offsetsToReport)
            {
                var graphitePath = $"{graphitePathPrefix}.{t.bladeKey}";
                graphiteClient.Send(graphitePath, (long)(now - t.currentGlobalOffsetTimestamp).TotalMilliseconds, now.ToDateTime());
            }
        }

        public void Shutdown()
        {
            periodicJobRunner.StopPeriodicJob(LagReportingJobName);

            foreach (var blade in blades)
                blade.Shutdown();
        }

        public void ExecuteFeeding()
        {
            foreach (var blade in blades)
                blade.ExecuteFeeding();
        }

        public void ResetLocalState()
        {
            foreach (var blade in blades)
                blade.ResetLocalState();
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            foreach (var blade in blades)
                blade.ExecuteForcedFeeding(delayUpperBound);
        }

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            return blades.All(blade => blade.AreEventsProcessedAt(timestamp));
        }

        private readonly IBlade[] blades;
        private readonly IGraphiteClient graphiteClient;
        private readonly IPeriodicJobRunner periodicJobRunner;
        private readonly string graphitePathPrefix;
    }
}