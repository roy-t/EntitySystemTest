﻿namespace EntitySystemTest.Benchmarks
{
    public class Benchmark
    {
        //public static List<string> Processed = new List<string>(10000);
        //private EarlyPipeline earlyPipeline;
        //private LatePipeline latePipeline;

        //[GlobalSetup]
        //public void Setup()
        //{
        //    this.earlyPipeline = this.CreateEarlyPipeline();
        //    this.latePipeline = this.CreateLatePipeline();
        //}

        //[Benchmark(Baseline = true)]
        //public void LatePipelineBenchmark()
        //   => this.latePipeline.Execute();


        //[Benchmark]
        //public void EarlyPipelineBenchmark()
        //    => this.earlyPipeline.Execute();


        //private EarlyPipeline CreateEarlyPipeline()
        //{
        //    var pipeline = new EarlyPipeline();
        //    var dictA = new Dictionary<int, ComponentA>
        //    {
        //        { 1, new ComponentA(1, "1-ONE-A") },
        //        { 2, new ComponentA(2, "2-ONE-A") },
        //        { 3, new ComponentA(3, "3-ONE-A") },
        //        { 4, new ComponentA(4, "4-ONE-A") }
        //    };

        //    var dictB = new Dictionary<int, ComponentB>
        //    {
        //        { 1, new ComponentB(1, "1-TWO-B") },
        //        { 3, new ComponentB(3, "3-TWO-B") },
        //        { 5, new ComponentB(5, "5-TWO-B") }
        //    };

        //    pipeline.AddComponentLookUp(dictA);
        //    pipeline.AddComponentLookUp(dictB);

        //    var systemA = new MonoSystem();
        //    pipeline.AddSystem(systemA);

        //    var systemB = new DuoSystem();
        //    pipeline.AddSystem(systemB);

        //    return pipeline;
        //}


        //public LatePipeline CreateLatePipeline()
        //{
        //    var pipeline = new LatePipeline();
        //    var dictA = new Dictionary<int, ComponentA>
        //    {
        //        { 1, new ComponentA(1, "1-ONE-A") },
        //        { 2, new ComponentA(2, "2-ONE-A") },
        //        { 3, new ComponentA(3, "3-ONE-A") },
        //        { 4, new ComponentA(4, "4-ONE-A") }
        //    };

        //    var dictB = new Dictionary<int, ComponentB>
        //    {
        //        { 1, new ComponentB(1, "1-TWO-B") },
        //        { 3, new ComponentB(3, "3-TWO-B") },
        //        { 5, new ComponentB(5, "5-TWO-B") }
        //    };

        //    pipeline.AddComponentLookUp(dictA);
        //    pipeline.AddComponentLookUp(dictB);

        //    var systemA = new MonoSystem();
        //    pipeline.AddSystem(systemA);

        //    var systemB = new DuoSystem();
        //    pipeline.AddSystem(systemB);

        //    return pipeline;
        //}
    }
}
