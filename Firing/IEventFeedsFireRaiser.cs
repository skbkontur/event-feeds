using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Core.EventFeeds.Firing
{
    public interface IEventFeedsFireRaiser
    {
        [NotNull]
        string Key { get; }

        [NotNull]
        IEnumerable<IEventFeed> Feeds();

        [NotNull]
        IEventFeedsFireRaiser NoParallel();

        void FirePeriodicTasks(TimeSpan actualizeInterval);

        void ExtinguishPeriodicTasks();
    }
}