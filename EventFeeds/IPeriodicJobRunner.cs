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
                                              [CanBeNull] Action onTakeTheLead,
                                              [CanBeNull] Action onLoseTheLead);

        void StopPeriodicJobWithLeaderElection([NotNull] string jobName);
    }
}