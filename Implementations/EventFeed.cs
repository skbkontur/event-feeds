using System;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

using SkbKontur.Graphite.Client;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class EventFeed
    {
        public EventFeed([NotNull] IBlade blade, IGraphiteClient graphiteClient, IPeriodicTaskRunner periodicTaskRunner)
            : this(blade.BladeId.BladeKey, new[] {blade}, graphiteClient, periodicTaskRunner)
        {
        }

        public EventFeed([NotNull] string feedKey, [NotNull, ItemNotNull] IBlade[] blades, IGraphiteClient graphiteClient, IPeriodicTaskRunner periodicTaskRunner)
        {
            FeedKey = feedKey;
            this.blades = blades;
            this.graphiteClient = graphiteClient;
            this.periodicTaskRunner = periodicTaskRunner;
            graphitePathPrefix = $"SubSystem.EventFeeds.ActualizationLag.{Environment.MachineName}";
        }

        [NotNull]
        public string FeedKey { get; }

        [NotNull]
        private string LagReportingTaskId => $"{FeedKey}-ReportActualizationLagJob";

        public void Initialize()
        {
            blades.ForEach(blade => blade.Initialize());
            periodicTaskRunner.Register(LagReportingTaskId, period : TimeSpan.FromMinutes(1), taskAction : ReportActualizationLagToGraphite);
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
            periodicTaskRunner.Unregister(LagReportingTaskId, timeout : 15000);
            blades.ForEach(blade => blade.Shutdown());
        }

        public void ExecuteFeeding()
        {
            blades.ForEach(blade => blade.ExecuteFeeding());
        }

        public void ResetLocalState()
        {
            blades.ForEach(blade => blade.ResetLocalState());
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            blades.ForEach(blade => blade.ExecuteForcedFeeding(delayUpperBound));
        }

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            return blades.All(blade => blade.AreEventsProcessedAt(timestamp));
        }

        private readonly IBlade[] blades;
        private readonly IGraphiteClient graphiteClient;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly string graphitePathPrefix;
    }
}