using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace EntitySystemTest
{
    public class Benchmark
    {
        public static List<string> Processed = new List<string>(10000);

        [Benchmark]
        public void EarlyPipelineBenchmark()
        {
            var pipeline = new EarlyPipeline();
            var dictA = new Dictionary<int, ComponentA>
            {
                { 1, new ComponentA(1, "1-ONE-A") },
                { 2, new ComponentA(2, "2-ONE-A") },
                { 3, new ComponentA(3, "3-ONE-A") },
                { 4, new ComponentA(4, "4-ONE-A") }
            };

            var dictB = new Dictionary<int, ComponentB>
            {
                { 1, new ComponentB(1, "1-TWO-B") },
                { 3, new ComponentB(3, "3-TWO-B") },
                { 5, new ComponentB(5, "5-TWO-B") }
            };

            pipeline.AddComponentLookUp(dictA);
            pipeline.AddComponentLookUp(dictB);

            var systemA = new MonoSystem();
            pipeline.AddSystem(systemA);

            var systemB = new DuoSystem();
            pipeline.AddSystem(systemB);

            for (var i = 0; i < 10000; i++)
            {
                pipeline.Execute();
            }
        }


        [Benchmark(Baseline = true)]
        public void LatePipelineBenchmark()
        {
            var pipeline = new LatePipeline();
            var dictA = new Dictionary<int, ComponentA>
            {
                { 1, new ComponentA(1, "1-ONE-A") },
                { 2, new ComponentA(2, "2-ONE-A") },
                { 3, new ComponentA(3, "3-ONE-A") },
                { 4, new ComponentA(4, "4-ONE-A") }
            };

            var dictB = new Dictionary<int, ComponentB>
            {
                { 1, new ComponentB(1, "1-TWO-B") },
                { 3, new ComponentB(3, "3-TWO-B") },
                { 5, new ComponentB(5, "5-TWO-B") }
            };

            pipeline.AddComponentLookUp(dictA);
            pipeline.AddComponentLookUp(dictB);

            var systemA = new MonoSystem();
            pipeline.AddSystem(systemA);

            var systemB = new DuoSystem();
            pipeline.AddSystem(systemB);

            for (var i = 0; i < 10000; i++)
            {
                pipeline.Execute();
            }
        }
    }
}
