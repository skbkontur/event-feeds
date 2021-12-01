using System;
using System.Diagnostics;
using System.Threading;

using Vostok.Logging.Abstractions;

namespace EventFeeds.Tests
{
    public class PeriodicJob : IDisposable
    {
        public PeriodicJob(string jobName,
                           TimeSpan delayBetweenIterations,
                           Action<CancellationToken> jobAction,
                           Action onTakeTheLead,
                           Action onLoseTheLead,
                           ILog logger,
                           CancellationToken cancellationToken)
        {
            this.jobName = jobName;
            this.delayBetweenIterations = delayBetweenIterations;
            this.jobAction = jobAction;
            this.onTakeTheLead = onTakeTheLead;
            this.onLoseTheLead = onLoseTheLead;
            this.logger = logger;
            jobCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            jobThread = new Thread(() => ThreadProc(jobCancellationTokenSource.Token))
                {
                    Name = jobName,
                    IsBackground = true,
                };
            jobThread.Start();
            this.logger.Info($"Job thread has started for: {jobName}");
        }

        public void Dispose()
        {
            jobCancellationTokenSource.Cancel();
            jobThread.Join();
            logger.Info($"Job thread has stopped for: {jobName}");

            jobCancellationTokenSource.Dispose();
        }

        private void ThreadProc(CancellationToken jobCancellationToken)
        {
            do
            {
                try
                {
                    logger.Info($"Leadership acquired for: {jobName}");
                    onTakeTheLead?.Invoke();
                    try
                    {
                        LeaderThreadProc(jobCancellationToken);
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
            } while (!jobCancellationToken.WaitHandle.WaitOne(delayBetweenIterations));
        }

        private void LeaderThreadProc(CancellationToken cancellationToken)
        {
            Stopwatch iterationStopwatch;
            do
            {
                iterationStopwatch = Stopwatch.StartNew();
                jobAction(cancellationToken);
            } while (!cancellationToken.WaitHandle.WaitOne(TimeSpanExtensions.Max(TimeSpan.Zero, delayBetweenIterations - iterationStopwatch.Elapsed)));
        }

        private readonly string jobName;
        private readonly TimeSpan delayBetweenIterations;
        private readonly Action<CancellationToken> jobAction;
        private readonly Action onTakeTheLead;
        private readonly Action onLoseTheLead;
        private readonly ILog logger;
        private readonly Thread jobThread;
        private readonly CancellationTokenSource jobCancellationTokenSource;
    }
}