using System;
using System.Diagnostics;
using System.Threading;

using Vostok.Logging.Abstractions;

namespace EventFeeds.Tests
{
    public class PeriodicJob
    {
        public PeriodicJob(string jobName,
                           TimeSpan delayBetweenIterations,
                           Action<CancellationToken> jobAction,
                           Action onTakeTheLead,
                           Action onLoseTheLead,
                           ILog logger)
        {
            this.jobName = jobName;
            this.delayBetweenIterations = delayBetweenIterations;
            this.jobAction = jobAction;
            this.onTakeTheLead = onTakeTheLead;
            this.onLoseTheLead = onLoseTheLead;
            this.logger = logger;
            stopSignal = new ManualResetEventSlim(false);
            jobThread = new Thread(ThreadProc)
                {
                    Name = jobName,
                    IsBackground = true,
                };
            jobThread.Start();
            this.logger.Info($"Job thread has started for: {jobName}");
        }

        public void Stop()
        {
            stopSignal.Set();
            jobThread.Join();
            logger.Info($"Job thread has stopped for: {jobName}");
        }

        private void ThreadProc()
        {
            do
            {
                try
                {
                    logger.Info($"Leadership acquired for: {jobName}");
                    onTakeTheLead?.Invoke();
                    try
                    {
                        LeaderThreadProc();
                    }
                    finally
                    {
                        onLoseTheLead?.Invoke();
                    }
                    logger.Info($"Leadership released for: {jobName}");
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Leadership lost with unhandled exception on job thread for: {jobName}");
                }
            } while (!stopSignal.Wait(delayBetweenIterations));
        }

        private void LeaderThreadProc()
        {
            Stopwatch iterationStopwatch;
            do
            {
                iterationStopwatch = Stopwatch.StartNew();
                jobAction(CancellationToken.None);
            } while (!stopSignal.Wait(TimeSpanExtensions.Max(TimeSpan.Zero, delayBetweenIterations - iterationStopwatch.Elapsed)));
        }

        private readonly string jobName;
        private readonly TimeSpan delayBetweenIterations;
        private readonly Action<CancellationToken> jobAction;
        private readonly Action onTakeTheLead;
        private readonly Action onLoseTheLead;
        private readonly ILog logger;
        private readonly ManualResetEventSlim stopSignal;
        private readonly Thread jobThread;
    }
}