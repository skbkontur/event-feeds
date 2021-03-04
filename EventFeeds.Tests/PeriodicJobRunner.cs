using System;
using System.Collections.Generic;
using System.Threading;

using SkbKontur.EventFeeds;

using Vostok.Logging.Abstractions;

namespace EventFeeds.Tests
{
    public class PeriodicJobRunner : IPeriodicJobRunner, IDisposable
    {
        public void RunPeriodicJobWithLeaderElection(string jobName,
                                                     TimeSpan delayBetweenIterations,
                                                     Action<CancellationToken> jobAction,
                                                     Func<IRunningEventFeed> onTakeTheLead,
                                                     Func<IRunningEventFeed> onLoseTheLead,
                                                     CancellationToken cancellationToken = default)
        {
            lock (this)
            {
                if (isDisposed)
                    throw new ObjectDisposedException("PeriodicJobRunnerWithLeaderElection is already disposed");
                if (runningJobs.ContainsKey(jobName))
                    throw new InvalidOperationException($"Job is already running: {jobName}");
                var runningJob = new PeriodicJob(jobName,
                                                 delayBetweenIterations,
                                                 jobAction,
                                                 onTakeTheLead : () => onTakeTheLead(),
                                                 onLoseTheLead : () => onLoseTheLead(),
                                                 new SilentLog(),
                                                 cancellationToken);
                runningJobs.Add(jobName, runningJob);
            }
        }

        public void StopPeriodicJobWithLeaderElection(string jobName)
        {
            lock (this)
            {
                if (isDisposed)
                    throw new ObjectDisposedException("PeriodicJobRunnerWithLeaderElection is already disposed");
                if (!runningJobs.ContainsKey(jobName))
                    throw new InvalidOperationException($"Job {jobName} does not exist");
                var job = runningJobs[jobName];
                runningJobs.Remove(jobName);
                job.Dispose();
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (isDisposed)
                    return;
                foreach (var runningJob in runningJobs.Values)
                    runningJob.Dispose();
                isDisposed = true;
            }
        }

        private bool isDisposed;
        private readonly Dictionary<string, PeriodicJob> runningJobs = new Dictionary<string, PeriodicJob>();
    }
}