using System;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeed.Interfaces;
using SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl;
using SKBKontur.Catalogue.Core.EventFeed.MultiRazorImpl.Interfaces;
using SKBKontur.Catalogue.Core.EventFeed.Providers;
using SKBKontur.Catalogue.Core.EventFeed.Providers.Elasticsearch;

namespace SKBKontur.Catalogue.Core.EventFeed.Recipes
{
    public class BusinessObjectToElasticsearchFeedRecipe
    {
        public BusinessObjectToElasticsearchFeedRecipe(
            [NotNull] MultiRazorEventFeedFactory eventFeedFactory,
            [NotNull] ElasticsearchEventFeedProviders elasticsearchEventFeedProviders,
            [NotNull] FileSystemEventFeedProviders fileSystemEventFeedProviders,
            [NotNull] IEventFeedsSettings eventFeedsSettings
            )
        {
            this.eventFeedFactory = eventFeedFactory;
            this.elasticsearchEventFeedProviders = elasticsearchEventFeedProviders;
            this.fileSystemEventFeedProviders = fileSystemEventFeedProviders;
            this.eventFeedsSettings = eventFeedsSettings;
        }

        [NotNull]
        public IEventFeeds CreateFeed<TEvent>(
            [NotNull] IEventConsumer<TEvent> consumer,
            [NotNull] string key,
            [NotNull] IEventLogEventSource<TEvent> eventSource,
            bool useInternalDataElasticsearch)
            where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            return CreateFeeds(consumer, key, eventSource, useInternalDataElasticsearch).Create().AsOneFeed();
        }

        [NotNull]
        public IEventFeeds CreateParallelFeeds<TEvent>(
            [NotNull] IEventConsumer<TEvent> consumer,
            [NotNull] string key,
            [NotNull] IEventLogEventSource<TEvent> eventSource) where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            return CreateFeeds(consumer, key, eventSource, false).Create();
        }

        private IEventFeedsBuilder<TEvent, long> CreateFeeds<TEvent>(IEventConsumer<TEvent> consumer, string key, IEventLogEventSource<TEvent> eventSource, bool useInternalDataElasticsearch) where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            var builder = eventFeedFactory
                .Feed2<TEvent, long>(key)
                .WithConsumer(consumer)
                .WithEventSource(eventSource)
                .WithOffsetStorageFactory(bladeContext =>
                                          elasticsearchEventFeedProviders
                                              .OffsetStorage<long>(bladeContext.BladeKey + "Offset", new ElasticsearchStorageSettings("EventFeedOffsets".CamelCaseForElasticsearch(), "MultiRazorEventFeedOffset", useInternalDataElasticsearch))
                                              .AndRollbackIfOffsetEmpty(TimeSpan.FromMinutes(10).Ticks))
                .AndUnprocessedEvents(fileSystemEventFeedProviders.CreateUnprocessedEventStorage<TEvent>(Path.Combine(eventFeedsSettings.UnprocessedEventsLocation, key)), c => c.AndLeaderElectionRequired())
                .WithBlade(key, c => c.WithDelay(TimeSpan.Zero).AndSendLagToGraphitePath(GetGraphitePath).AndLeaderElectionRequired());

            var i = 0;
            var blades = new[] {TimeSpan.Zero, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), TimeSpan.FromMinutes(6), TimeSpan.FromMinutes(15)};
            foreach(var timeSpan in blades)
            {
                var delay = timeSpan;
                builder = builder.WithBlade(key + "_Blade" + i, c => c.WithDelay(delay).AndSendLagToGraphitePath(GetGraphitePath).AndLeaderElectionRequired());
                i++;
            }
            return builder;
        }

        [NotNull]
        private static string GetGraphitePath([NotNull] IBladeConfigurationContext bladeContext)
        {
            return string.Format("EDI.SubSystem.Storage.Elasticsearch.ActualizationLag.{1}.{0}", Environment.MachineName, bladeContext.BladeKey);
        }

        private readonly MultiRazorEventFeedFactory eventFeedFactory;
        private readonly ElasticsearchEventFeedProviders elasticsearchEventFeedProviders;
        private readonly FileSystemEventFeedProviders fileSystemEventFeedProviders;
        private readonly IEventFeedsSettings eventFeedsSettings;
    }
}