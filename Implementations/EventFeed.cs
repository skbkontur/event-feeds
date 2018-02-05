using System;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Catalogue.Core.Graphite.Client.Relay;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeeds.Implementations
{
    public class EventFeed : IEventFeed
    {
        public EventFeed([NotNull] IBlade blade, ICatalogueGraphiteClient graphiteClient, IPeriodicTaskRunner periodicTaskRunner)
            : this(blade.BladeId.BladeKey, new[] {blade}, graphiteClient, periodicTaskRunner)
        {
        }

        public EventFeed([NotNull] string feedKey, [NotNull, ItemNotNull] IBlade[] blades, ICatalogueGraphiteClient graphiteClient, IPeriodicTaskRunner periodicTaskRunner)
        {
            FeedKey = feedKey;
            this.blades = blades;
            this.graphiteClient = graphiteClient;
            this.periodicTaskRunner = periodicTaskRunner;
        }

        [NotNull]
        public string FeedKey { get; }

        [NotNull]
        private string LagReportingTaskId => $"{FeedKey}-ReportActualizationLagJob";

        public void Initialize()
        {
            lock(locker)
            {
                blades.ForEach(blade => blade.Initialize());
                periodicTaskRunner.Register(LagReportingTaskId, period : TimeSpan.FromMinutes(1), taskAction : () => blades.ForEach(ReportActualizationLagToGraphite));
            }
        }

        private void ReportActualizationLagToGraphite([NotNull] IBlade blade)
        {
            var currentGlobalOffsetTimestamp = blade.GetCurrentGlobalOffsetTimestamp();
            if(currentGlobalOffsetTimestamp != null)
            {
                var now = Timestamp.Now;
                var graphitePath = $"{graphitePathPrefix}.{blade.BladeId.BladeKey}";
                graphiteClient.Send(graphitePath, (long)(now - currentGlobalOffsetTimestamp).TotalMilliseconds, now.ToDateTime());
            }
        }

        public void Shutdown()
        {
            lock(locker)
            {
                periodicTaskRunner.Unregister(LagReportingTaskId, timeout : 15000);
                blades.ForEach(blade => blade.Shutdown());
            }
        }

        public void ExecuteFeeding()
        {
            lock(locker)
            {
                blades.ForEach(blade => blade.ExecuteFeeding());
            }
        }

        public void ResetLocalState()
        {
            lock(locker)
            {
                blades.ForEach(blade => blade.ResetLocalState());
            }
        }

        public void ExecuteForcedFeeding(TimeSpan delayUpperBound)
        {
            lock(locker)
            {
                blades.ForEach(blade => blade.ExecuteForcedFeeding(delayUpperBound));
            }
        }

        public bool AreEventsProcessedAt([NotNull] Timestamp timestamp)
        {
            lock(locker)
            {
                return blades.All(blade => blade.AreEventsProcessedAt(timestamp));
            }
        }

        private readonly IBlade[] blades;
        private readonly ICatalogueGraphiteClient graphiteClient;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly string graphitePathPrefix = $"EDI.SubSystem.EventFeeds.ActualizationLag.{Environment.MachineName}";
        private readonly object locker = new object();
    }
}