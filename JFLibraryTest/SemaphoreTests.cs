using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Diagnostics;
using JFLibrary;

namespace JFLibraryTest
{

    [TestClass]
    public class SemaphoreTests
    {
        // See blog post for detailed explanation of tests:
        // http://joelfillmore.com/throttling-web-api-calls/

        // helper method to simulate work on the thread
        static void DoWork(int taskId)
        {
            DateTime started = DateTime.Now;
            Thread.Sleep(300);  // simulate work
            Console.WriteLine(
                "Task {0} started {1}, completed {2}",
                taskId,
                started.ToString("ss.fff"),
                DateTime.Now.ToString("ss.fff"));
        }

        [TestMethod]
        public void StandardSemaphoreTest()
        {
            using (SemaphoreSlim pool = new SemaphoreSlim(5))
            {
                for (int i = 1; i <= 6; i++)
                {
                    Thread t = new Thread(new ParameterizedThreadStart((taskId) =>
                    {
                        pool.Wait();
                        DoWork((int)taskId);
                        pool.Release();
                    }));
                    t.Start(i);
                }

                Thread.Sleep(2000); // give all the threads a chance to finish

                // NOTE: view the console output in Test Explorer
            }
        }

        [TestMethod]
        public void TimeSpanSemaphoreTest()
        {
            using (var throttle = new TimeSpanSemaphore(5, TimeSpan.FromSeconds(1)))
            {
                for (int i = 1; i <= 6; i++)
                {
                    Thread t = new Thread(new ParameterizedThreadStart((taskId) =>
                    {
                        throttle.Run(
                            () => DoWork((int)taskId),
                            CancellationToken.None);
                    }));
                    t.Start(i);
                }

                Thread.Sleep(2000); // give all the threads a chance to finish

                // NOTE: view the console output in Test Explorer
            }
        }

    }
}
