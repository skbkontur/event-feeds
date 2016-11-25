using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SKBKontur.Catalogue.Core.EventFeeds.Implementations;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeeds.Firing
{
    public class EventFeedsFireRaiser : IEventFeedsFireRaiser
    {
        public EventFeedsFireRaiser(
            [NotNull] string key,
            [NotNull] List<IEventFeed> feeds,
            [NotNull] IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection,
            [NotNull] IPeriodicTaskRunner periodicTaskRunner,
            [NotNull] IEventFeedsSettings eventFeedsSettings)
        {
            if(feeds.Count == 0)
                throw new InvalidProgramStateException(string.Format("EventFeedsFireRaiser (key = {0}) can't be created without feeds", key));
            this.periodicJobRunnerWithLeaderElection = periodicJobRunnerWithLeaderElection;
            this.periodicTaskRunner = periodicTaskRunner;
            this.eventFeedsSettings = eventFeedsSettings;
            this.feeds = feeds;
            Key = key;
        }

        [NotNull]
        public string Key { get; private set; }

        [NotNull]
        public IEnumerable<IEventFeed> Feeds()
        {
            return feeds;
        }

        [NotNull]
        public IEventFeedsFireRaiser NoParallel()
        {
            if(feeds.Count == 1)
                return this;
            var compositeFeed = new CompositeEventFeed(Key, feeds);
            return new EventFeedsFireRaiser(Key, new List<IEventFeed> {compositeFeed}, periodicJobRunnerWithLeaderElection, periodicTaskRunner, eventFeedsSettings);
        }

        public void FirePeriodicTasks()
        {
            foreach(var eventFeed in feeds)
            {
                EventFeedsRegistry.Register(eventFeed);
                var feed = eventFeed;
                if(eventFeed.LeaderElectionRequired)
                    periodicJobRunnerWithLeaderElection.RunPeriodicJob(feed.Key + "Indexer", eventFeedsSettings.ActualizeInterval, feed.ExecuteFeeding, feed.Initialize, feed.Shutdown);
                else
                {
                    var initialized = false;
                    periodicTaskRunner.Register(feed.Key + "Indexer",
                                                eventFeedsSettings.ActualizeInterval, () =>
                                                    {
                                                        if(!initialized)
                                                        {
                                                            feed.Initialize();
                                                            initialized = true;
                                                        }
                                                        feed.ExecuteFeeding();
                                                    });
                }
            }
        }

        public void ExtinguishPeriodicTasks()
        {
            foreach(var eventFeed in feeds)
            {
                var feed = eventFeed;
                if(eventFeed.LeaderElectionRequired)
                    periodicJobRunnerWithLeaderElection.StopPeriodicJob(feed.Key + "Indexer");
                else
                    periodicTaskRunner.Unregister(feed.Key + "Indexer", (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                EventFeedsRegistry.Unregister(eventFeed.Key);
            }
        }

        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly IEventFeedsSettings eventFeedsSettings;

        private readonly List<IEventFeed> feeds;
    }
}