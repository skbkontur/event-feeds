using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;
using SkbKontur.EventFeeds.Implementations;

using Vostok.Logging.Abstractions;

namespace EventFeeds.Tests
{
    public class BasicEventFeedTests
    {
        [SetUp]
        public void SetUp()
        {
            eventSource = new TestEventSource();
            eventConsumer = new TestEventConsumer();
        }

        [Test]
        public void OneBlade_ShouldConsumeAllEventsExactlyOnce_WhenEventsAreNotWrittenInThePast()
        {
            var bladeDelays = new[] {TimeSpan.Zero};
            var eventFeedsRunner = RunEventFeeds(bladeDelays);

            var producedEvents = ProduceEvents(writeEventsInThePastDelay : null);
            ExecuteEventFeedingToEnd(eventFeedsRunner, bladeDelays);

            eventConsumer.ConsumedEvents.Count.Should().Be(producedEvents.Count);
            eventConsumer.ConsumedEvents.Select(x => x.Event).ToList().Should().BeEquivalentTo(producedEvents);
        }

        [Test]
        public void OneBlade_ShouldMissEvents_WhenEventsAreWrittenInThePast()
        {
            var bladeDelays = new[] {TimeSpan.Zero};
            var eventFeedsRunner = RunEventFeeds(bladeDelays);

            var producedEvents = ProduceEvents(writeEventsInThePastDelay : TimeSpan.FromMilliseconds(30));
            ExecuteEventFeedingToEnd(eventFeedsRunner, bladeDelays);

            eventConsumer.ConsumedEvents.Count.Should().BeLessThan(producedEvents.Count);
            eventConsumer.ConsumedEvents.Select(x => x.Event).ToList().Should().NotBeEquivalentTo(producedEvents);
        }

        [Test]
        public void TwoBlades_ShouldConsumeAllEvents_EvenWhenEventsAreWrittenInThePast()
        {
            var bladeDelays = new[] {TimeSpan.Zero, TimeSpan.FromMilliseconds(50)};
            var eventFeedsRunner = RunEventFeeds(bladeDelays);

            var producedEvents = ProduceEvents(writeEventsInThePastDelay : TimeSpan.FromMilliseconds(30));
            ExecuteEventFeedingToEnd(eventFeedsRunner, bladeDelays);

            eventConsumer.ConsumedEvents.Count.Should().BeGreaterThan(producedEvents.Count);
            eventConsumer.ConsumedEvents.Select(x => x.Event).Distinct().ToList().Should().BeEquivalentTo(producedEvents);
        }

        private IEventFeedsRunner RunEventFeeds(TimeSpan[] bladeDelays)
        {
            IBladesBuilder<long?> bladesBuilder = BladesBuilder.New(eventSource, eventConsumer, new SilentLog());
            for (var i = 0; i < bladeDelays.Length; i++)
                bladesBuilder = bladesBuilder.WithBlade($"Blade_{i}", bladeDelays[i]);

            var eventFeedsRunner = eventFeedFactory.WithOffsetType<long?>()
                                                   .WithEventType(bladesBuilder)
                                                   .WithOffsetStorageFactory(bladeId => new InMemoryOffsetStorage<long?>())
                                                   .RunFeeds(delayBetweenIterations : TimeSpan.FromMilliseconds(20));
            return eventFeedsRunner;
        }

        private List<int> ProduceEvents(TimeSpan? writeEventsInThePastDelay)
        {
            var eventProducerTask = Task.Run(async () =>
                {
                    var eventValue = 0;
                    var sw = Stopwatch.StartNew();
                    while (sw.Elapsed < TimeSpan.FromSeconds(5))
                    {
                        eventSource.WriteEvent(Timestamp.Now, eventValue++);

                        if (writeEventsInThePastDelay != null)
                            eventSource.WriteEvent(Timestamp.Now - writeEventsInThePastDelay.Value, eventValue++);

                        await Task.Delay(TimeSpan.FromMilliseconds(10));
                    }
                });
            eventProducerTask.Wait();

            eventSource.Timeline.Should().NotBeEmpty();

            var producedEvents = eventSource.Timeline.Select(x => x.Event).ToList();
            return producedEvents;
        }

        private void ExecuteEventFeedingToEnd(IEventFeedsRunner eventFeedsRunner, TimeSpan[] bladeDelays)
        {
            eventFeedsRunner.ExecuteForcedFeeding(delayUpperBound : bladeDelays.Max());
            eventFeedsRunner.Stop();

            Console.Out.WriteLine($"producedEventsCount: {eventSource.Timeline.Count}, consumedEventsCount: {eventConsumer.ConsumedEvents.Count}");
        }

        private TestEventSource eventSource;
        private TestEventConsumer eventConsumer;
        private readonly EventFeedFactory eventFeedFactory = new EventFeedFactory(new GlobalTimeProvider(), new PeriodicJobRunner());
    }
}