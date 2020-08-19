#if RELEASE
using BenchmarkDotNet.Running;
#endif
using System;

namespace EntitySystemTest
{
    /*
     * Approach #2, what if we do all the reflection when a system is added
     * we store the number of arguments and for each arugment where to look it up
     * we can even lookup the Process method in advance so we can call it more easily
     * or we could maybe even construct a special "caller" that just casts all the objects to the right type?
     * Maybe benchmark what is better! (Though benchmarkdotnet is being an ass!)
     */


    class Program
    {
        static void Main()
        {
            Console.WriteLine("Hello World!");


            var pipeline = PipelineBuilder.ExampleSetup();


            //#if RELEASE
            //            BenchmarkRunner.Run<Benchmark>();

            //#else
            //            var benchmark = new Benchmark();
            //            benchmark.EarlyPipelineBenchmark();
            //            benchmark.LatePipelineBenchmark();            
            //#endif

            Console.WriteLine($"Counted: {MonoSystem.Counter}, {DuoSystem.Counter}");
            Console.ReadLine();
        }
    }

    public interface ISystem { }


    public interface ISystem<T0> : ISystem
        where T0 : IComponent
    {
        public void Process(T0 component);
    }

    public interface ISystem<T0, T1> : ISystem
       where T0 : IComponent
        where T1 : IComponent
    {
        public void Process(T0 component, T1 component2);
    }

    public interface IComponent
    {
        int Entity { get; }
    }

    public class ComponentA : IComponent
    {
        public ComponentA(int entity, string name)
        {
            this.Entity = entity;
            this.Name = name;
        }
        public int Entity { get; set; }
        public string Name { get; }
    }

    public class ComponentB : IComponent
    {
        public ComponentB(int entity, string name)
        {
            this.Entity = entity;
            this.Name = name;
        }
        public int Entity { get; }
        public string Name { get; }
    }

    public class SoloSystem : ISystem
    {
        public static long Counter { get; set; }

        public void Process() => Counter++;
    }

    public class MonoSystem : ISystem<ComponentA>
    {
        public static long Counter { get; set; }

        public void Process(ComponentA component) => Counter++;
    }
    public class DuoSystem : ISystem<ComponentA, ComponentB>
    {
        public static long Counter { get; set; }

        public void Process(ComponentA component, ComponentB component2) => Counter++;
    }
}
