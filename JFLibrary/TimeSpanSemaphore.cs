using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace JFLibrary
{
    /// <summary>
    /// Allows a limited number of acquisitions during a timespan
    /// </summary>
    public class TimeSpanSemaphore : IDisposable
    {
        private SemaphoreSlim _pool;

        // the time span for the max number of callers
        private TimeSpan _resetSpan;

        // keep track of the release times
        private Queue<DateTime> _releaseTimes;

        // protect release time queue
        private object _queueLock = new object();

        public TimeSpanSemaphore(int maxCount, TimeSpan resetSpan)
        {
            _pool = new SemaphoreSlim(maxCount, maxCount);
            _resetSpan = resetSpan;

            // initialize queue with old timestamps
            _releaseTimes = new Queue<DateTime>(maxCount);
            for (int i = 0; i < maxCount; i++)
            {
                _releaseTimes.Enqueue(DateTime.MinValue);
            }
        }

        /// <summary>
        /// Blocks the current thread until it can enter the semaphore, while observing a CancellationToken
        /// </summary>
        private void Wait(CancellationToken cancelToken)
        {
            // will throw if token is cancelled
            _pool.Wait(cancelToken);

            // get the oldest release from the queue
            DateTime oldestRelease;
            lock (_queueLock)
            {
                oldestRelease = _releaseTimes.Dequeue();
            }

            // sleep until the time since the previous release equals the reset period
            DateTime now = DateTime.UtcNow;
            DateTime windowReset = oldestRelease.Add(_resetSpan);
            if (windowReset > now)
            {
                int sleepMilliseconds = Math.Max(
                    (int)(windowReset.Subtract(now).Ticks / TimeSpan.TicksPerMillisecond),
                    1); // sleep at least 1ms to be sure next window has started
                Debug.WriteLine("Waiting {0} ms for TimeSpanSemaphore limit to reset.", sleepMilliseconds);

                bool cancelled = cancelToken.WaitHandle.WaitOne(sleepMilliseconds);
                if (cancelled)
                {
                    Release();
                    cancelToken.ThrowIfCancellationRequested();
                }
            }
        }

        /// <summary>
        /// Exits the semaphore
        /// </summary>
        private void Release()
        {
            lock (_queueLock)
            {
                _releaseTimes.Enqueue(DateTime.UtcNow);
            }
            _pool.Release();
        }

        /// <summary>
        /// Runs an action after entering the semaphore (if the CancellationToken is not canceled)
        /// </summary>
        public void Run(Action action, CancellationToken cancelToken)
        {
            // will throw if token is cancelled, but will auto-release lock
            Wait(cancelToken);

            try
            {
                action();
            }
            finally
            {
                Release();
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance
        /// </summary>
        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
