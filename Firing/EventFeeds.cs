using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeed.Interfaces;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl
{
    public class EventFeeds : IEventFeeds
    {
        public EventFeeds(
            [NotNull] string key,
            [NotNull] List<IEventFeed> feeds,
            [NotNull] IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection,
            [NotNull] IPeriodicTaskRunner periodicTaskRunner,
            [NotNull] IEventFeedsSettings eventFeedsSettings)
        {
            if(feeds.Count == 0)
                throw new InvalidProgramStateException(string.Format("EventFeeds (key = {0}) can't be created without feeds", key));
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
        public IEventFeeds AsOneFeed()
        {
            if(feeds.Count == 1)
                return this;
            var compositeFeed = new CompositeEventFeed(Key, feeds);
            return new EventFeeds(Key, new List<IEventFeed> {compositeFeed}, periodicJobRunnerWithLeaderElection, periodicTaskRunner, eventFeedsSettings);
        }

        public void RegisterPeriodicTasks()
        {
            foreach(var eventFeed in feeds)
            {
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

        public void UnregisterPeriodicTasks()
        {
            foreach(var eventFeed in feeds)
            {
                var feed = eventFeed;
                if(eventFeed.LeaderElectionRequired)
                    periodicJobRunnerWithLeaderElection.StopPeriodicJob(feed.Key + "Indexer");
                else
                    periodicTaskRunner.Unregister(feed.Key + "Indexer", (int)TimeSpan.FromMinutes(1).TotalMilliseconds);
            }
        }

        private readonly IPeriodicJobRunnerWithLeaderElection periodicJobRunnerWithLeaderElection;
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly IEventFeedsSettings eventFeedsSettings;

        private readonly List<IEventFeed> feeds;
    }

    public static class EventFeedsExtensions
    {
        [NotNull]
        public static IEventFeeds AddFeedsToRegistry([NotNull] this IEventFeeds eventFeeds, [NotNull] IEventFeedRegistry registry)
        {
            foreach(var eventFeed in eventFeeds.Feeds())
                registry.Register(eventFeed);
            return eventFeeds;
        }
    }
}