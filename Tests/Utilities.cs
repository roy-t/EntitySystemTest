using System;
using System.Collections.Generic;
using System.Threading;

namespace Tests
{
    internal static class ListExtensions
    {
        [ThreadStatic] public static Random Random = new Random(unchecked((Environment.TickCount * 31) + Thread.CurrentThread.ManagedThreadId));

        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
