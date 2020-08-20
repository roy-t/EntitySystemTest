using System;
using BenchmarkDotNet.Running;
using EntitySystemTest.Benchmarks;

namespace EntitySystemTest
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<Benchmark>();
            Console.ReadLine();
        }
    }
}
