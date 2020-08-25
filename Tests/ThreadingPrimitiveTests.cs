using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntitySystemTest.Threading;
using NUnit.Framework;

namespace Tests
{
    public class ThreadingPrimitiveTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(1000);

        [TestCase(0)]
        [TestCase(1)]
        public void ShouldLock(int threadIndex)
        {
            var primitive = new ThreadingPrimitive(2);
            Assert.That(async () => await RunWithTimeout(() => primitive.DecrementAndWait(threadIndex)), Throws.TypeOf<TimeoutException>());
        }

        [Test]
        public void ShouldContinue()
        {
            var primitive = new ThreadingPrimitive(1);
            Assert.That(async () => await RunWithTimeout(() => primitive.DecrementAndWait(0)), Throws.Nothing);
        }

        [TestCase(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [TestCase(new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 })]
        public void ShouldContinueAfterMultipleThreadsDecrement(int[] threadIds)
        {
            var primitive = new ThreadingPrimitive(10);
            Assert.That(async () => await RunWithTimeout(
                () => primitive.DecrementAndWait(threadIds[0]),
                () => primitive.DecrementAndWait(threadIds[1]),
                () => primitive.DecrementAndWait(threadIds[2]),
                () => primitive.DecrementAndWait(threadIds[3]),
                () => primitive.DecrementAndWait(threadIds[4]),
                () => primitive.DecrementAndWait(threadIds[5]),
                () => primitive.DecrementAndWait(threadIds[6]),
                () => primitive.DecrementAndWait(threadIds[7]),
                () => primitive.DecrementAndWait(threadIds[8]),
                () => primitive.DecrementAndWait(threadIds[9])), Throws.Nothing);
        }

        private static async Task RunWithTimeout(params Action[] actions)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var delayTask = Task.Delay(Timeout, cancellationTokenSource.Token);
            var tasks = actions
                .Select(a => Task.Run(a, cancellationTokenSource.Token))
                .Append(delayTask)
                .ToList();

            if (delayTask == await Task.WhenAny(tasks))
            {
                cancellationTokenSource.Cancel();
                throw new TimeoutException();
            }
            else
            {
                cancellationTokenSource.Cancel();
            }
        }
    }
}
