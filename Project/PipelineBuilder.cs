using System;
using System.Collections.Generic;
using System.Linq;
using PlainDictionary = System.Collections.IDictionary;

namespace EntitySystemTest
{
    public sealed class PipelineBuilder
    {
        private readonly List<SystemSpec> SystemSpecs;
        private readonly Dictionary<Type, PlainDictionary> ComponentContainers;

        public PipelineBuilder()
        {
            this.SystemSpecs = new List<SystemSpec>();
            this.ComponentContainers = new Dictionary<Type, PlainDictionary>();
        }

        public SystemSpec AddSystem<T>()
            where T : ISystem, new()
        {
            var spec = new SystemSpec(typeof(T));
            this.SystemSpecs.Add(spec);
            return spec;
        }

        public PipelineBuilder AddComponentContainer<T>(Dictionary<int, T> componentLookUp)
             where T : IComponent
        {
            this.ComponentContainers.Add(typeof(T), componentLookUp);
            return this;
        }

        private ParallelPipeline Build()
        {
            var ordered = CoffmanGrahamOrderer.Order(this.SystemSpecs);
            var stages = CoffmanGrahamOrderer.DivideIntoStages(ordered);
            return null; // TODO: see EarlyPipeline on how to set-up the pipeline
        }

        public static ParallelPipeline ExampleSetup()
        {
            var builder = new PipelineBuilder();

            builder.AddSystem<SoloSystem>()
                .Produces("GBuffer", "Cleared")
                .InSequence();

            builder.AddSystem<MonoSystem>()
                .Requires("GBuffer", "Cleared")
                .Produces("GBuffer", "Materials")
                .InSequence();

            builder.AddSystem<DuoSystem>()
                .Requires("GBuffer", "Materials")
                .Produces("GBuffer", "Point-Lights")
                .InSequence();

            builder.AddSystem<MonoSystem>()
                .Requires("GBuffer", "Cleared")
                .Requires("Foo", "Bar")
                .Produces("Zzz", "yyy")
                .Parallel();

            builder.AddSystem<MonoSystem>()
                .Requires("GBuffer", "Point-Lights")
                .Produces("Foo", "Bar")
                .Parallel();

            builder.AddSystem<MonoSystem>()
                .Requires("GBuffer", "Point-Lights")
                .Produces("Foo", "Bar2")
                .Parallel();

            builder.AddSystem<MonoSystem>()
                .Requires("Foo", "Bar")
                .Requires("Foo", "Bar2")
                .InSequence();

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

            builder.AddComponentContainer(dictA)
                   .AddComponentContainer(dictB);

            return builder.Build();
        }
    }

    public sealed class Job
    {
        public Job(SystemBinding systemBinding)
        {
            this.SystemBinding = systemBinding;
        }

        public SystemBinding SystemBinding { get; }
    }

    public sealed class Stage
    {
        public Stage(IReadOnlyList<SystemSpec> systemSpecs)
        {
            this.SystemSpecs = systemSpecs;
        }

        public IReadOnlyList<SystemSpec> SystemSpecs { get; }

        public override string ToString()
            => $"stage with {this.SystemSpecs.Count} spec(s): " +
            $"requires [{string.Join(", ", this.SystemSpecs.SelectMany(s => s.RequiredResources).ToHashSet())}], " +
            $"produces [{string.Join(", ", this.SystemSpecs.SelectMany(s => s.ProducedResources).ToHashSet())}]";
    }

    public sealed class ParallelPipeline
    {

        public ParallelPipeline()
        {

        }
    }

    public sealed class SystemSpec
    {
        private readonly List<ResourceState> RequiresList;
        private readonly List<ResourceState> ProducesList;

        public SystemSpec(Type systemType)
        {
            this.SystemType = systemType;
            this.RequiresList = new List<ResourceState>();
            this.ProducesList = new List<ResourceState>();
        }

        public Type SystemType { get; }

        public bool AllowParallelism { get; set; }

        internal IReadOnlyList<ResourceState> RequiredResources => this.RequiresList;
        internal IReadOnlyList<ResourceState> ProducedResources => this.ProducesList;

        public SystemSpec Requires(string resource, string state)
        {
            this.RequiresList.Add(new ResourceState(resource, state));
            return this;
        }

        public SystemSpec Produces(string resource, string state)
        {
            this.ProducesList.Add(new ResourceState(resource, state));
            return this;
        }

        public SystemSpec Parallel()
        {
            this.AllowParallelism = true;
            return this;
        }


        public SystemSpec InSequence()
        {
            this.AllowParallelism = false;
            return this;
        }

        public override string ToString()
            => $"{this.SystemType.Name}: " +
            $"requires: [{string.Join(", ", this.RequiredResources)}], " +
            $"produces: [{string.Join(", ", this.ProducedResources)}]";
    }

    public sealed class ResourceState
    {
        public ResourceState(string resource, string state)
        {
            this.Resource = resource;
            this.State = state;
        }

        public string Resource { get; }
        public string State { get; }

        public override bool Equals(object obj) => obj is ResourceState state && this.Resource == state.Resource && this.State == state.State;
        public override int GetHashCode() => HashCode.Combine(this.Resource, this.State);

        public override string ToString() => $"({this.Resource} - {this.State})";
    }
}
