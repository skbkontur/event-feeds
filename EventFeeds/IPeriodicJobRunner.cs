using System;
using System.Threading;

using JetBrains.Annotations;

namespace SkbKontur.EventFeeds
{
    [PublicAPI]
    public interface IPeriodicJobRunner
    {
        void RunPeriodicJobWithLeaderElection([NotNull] string jobName,
                                              TimeSpan delayBetweenIterations,
                                              [NotNull] Action<CancellationToken> jobAction,
                                              [NotNull] Func<IRunningEventFeed> onTakeTheLead,
                                              [NotNull] Func<IRunningEventFeed> onLoseTheLead);

        void StopPeriodicJobWithLeaderElection([NotNull] string jobName);
    }
}