using System;
using System.IO;
using JetBrains.Annotations;
using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.Core.EventFeeds.Building;
using SKBKontur.Catalogue.Core.EventFeeds.Firing;
using SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages;
using SKBKontur.Catalogue.Core.EventFeeds.UnprocessedEvents;

namespace SKBKontur.Catalogue.Core.EventFeeds.Receipes
{
    public class BusinessObjectToElasticsearchFeedRecipe
    {
        public BusinessObjectToElasticsearchFeedRecipe(
            [NotNull] MultiRazorEventFeedFactory eventFeedFactory,
            [NotNull] ElasticsearchOffsetStorageProvider elasticsearchOffsetStorageProvider,
            [NotNull] FileSystemUnprocessedEventsStorageProvider fileSystemUnprocessedEventsStorageProvider,
            [NotNull] IEventFeedsSettings eventFeedsSettings
            )
        {
            this.eventFeedFactory = eventFeedFactory;
            this.elasticsearchOffsetStorageProvider = elasticsearchOffsetStorageProvider;
            this.fileSystemUnprocessedEventsStorageProvider = fileSystemUnprocessedEventsStorageProvider;
            this.eventFeedsSettings = eventFeedsSettings;
        }

        [NotNull]
        public IEventFeedsFireRaiser CreateFeed<TEvent>(
            [NotNull] IEventConsumer<TEvent> consumer,
            [NotNull] string key,
            [NotNull] IEventSource<TEvent> eventSource,
            bool useInternalDataElasticsearch)
            where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            return CreateFeeds(consumer, key, eventSource, useInternalDataElasticsearch).Create().NoParallel();
        }

        [NotNull]
        public IEventFeedsFireRaiser CreateParallelFeeds<TEvent>(
            [NotNull] IEventConsumer<TEvent> consumer,
            [NotNull] string key,
            [NotNull] IEventSource<TEvent> eventSource) where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            return CreateFeeds(consumer, key, eventSource, false).Create();
        }

        private IEventFeedsBuilder<TEvent, long> CreateFeeds<TEvent>(IEventConsumer<TEvent> consumer, string key, IEventSource<TEvent> eventSource, bool useInternalDataElasticsearch) where TEvent : GenericEvent, ICanSplitToElementary<TEvent>
        {
            return eventFeedFactory
                .Feed<TEvent, long>(key)
                .WithConsumer(consumer)
                .WithEventSource(eventSource)
                .WithOffsetStorageFactory(bladeContext =>
                                          elasticsearchOffsetStorageProvider
                                              .OffsetStorage<long>(bladeContext.BladeKey + "Offset", new ElasticsearchStorageSettings("EventFeedOffsets".CamelCaseForElasticsearch(), "MultiRazorEventFeedOffset", useInternalDataElasticsearch))
                                              .AndRollbackIfOffsetEmpty(TimeSpan.FromMinutes(10).Ticks))
                .AndUnprocessedEvents(fileSystemUnprocessedEventsStorageProvider.CreateUnprocessedEventStorage<TEvent>(Path.Combine(eventFeedsSettings.UnprocessedEventsLocation, key)), c => c.AndLeaderElectionRequired())
                .WithBlade(key, c => c.WithDelay(TimeSpan.Zero).AndSendLagToGraphitePath(GetGraphitePath).AndLeaderElectionRequired())
                .WithBlade(key + "_Blade0", c => c.WithDelay(TimeSpan.FromSeconds(15)).AndSendLagToGraphitePath(GetGraphitePath).AndLeaderElectionRequired())
                .WithBlade(key + "_Blade1", c => c.WithDelay(TimeSpan.FromMinutes(15)).AndSendLagToGraphitePath(GetGraphitePath).AndLeaderElectionRequired());
        }

        [NotNull]
        private static string GetGraphitePath([NotNull] IBladeConfigurationContext bladeContext)
        {
            return string.Format("EDI.SubSystem.Storage.Elasticsearch.ActualizationLag.{1}.{0}", Environment.MachineName, bladeContext.BladeKey);
        }

        private readonly MultiRazorEventFeedFactory eventFeedFactory;
        private readonly ElasticsearchOffsetStorageProvider elasticsearchOffsetStorageProvider;
        private readonly FileSystemUnprocessedEventsStorageProvider fileSystemUnprocessedEventsStorageProvider;
        private readonly IEventFeedsSettings eventFeedsSettings;
    }
}