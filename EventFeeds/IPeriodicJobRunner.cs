using System;

using JetBrains.Annotations;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public interface IPeriodicJobRunner
    {
        void RunPeriodicJobWithLeaderElection([NotNull] string jobName,
                                              TimeSpan delayBetweenIterations,
                                              [NotNull] Action jobAction,
                                              [NotNull] Func<IRunningEventFeed> onTakeTheLead,
                                              [NotNull] Func<IRunningEventFeed> onLoseTheLead);

        void StopPeriodicJobWithLeaderElection([NotNull] string jobName);
    }
}