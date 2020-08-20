using System;
using System.Collections.Generic;
using EntitySystemTest.Systems;
using LightInject;
using PlainDictionary = System.Collections.IDictionary;

namespace EntitySystemTest.Pipeline
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
            var spec = SystemSpec.Construct<T>();
            this.SystemSpecs.Add(spec);
            return spec;
        }

        public PipelineBuilder AddComponentContainer<T>(Dictionary<int, T> componentLookUp)
             where T : IComponent
        {
            this.ComponentContainers.Add(typeof(T), componentLookUp);
            return this;
        }

        public ParallelPipeline Build(ServiceContainer serviceContainer)
        {
            var ordered = CoffmanGrahamOrderer.Order(this.SystemSpecs);
            var stages = CoffmanGrahamOrderer.DivideIntoStages(ordered);

            var pipelineStages = new List<PipelineStage>();
            for (var i = 0; i < stages.Count; i++)
            {
                var systemBindings = new List<ISystemBinding>();
                var stage = stages[i];
                for (var j = 0; j < stage.Count; j++)
                {
                    var systemSpec = stage[j];
                    var system = (ISystem)serviceContainer.GetInstance(systemSpec.SystemType);
                    systemBindings.AddRange(SystemBinder.BindSystem(system, this.ComponentContainers));

                }

                pipelineStages.Add(new PipelineStage(systemBindings));
            }

            return new ParallelPipeline(pipelineStages);
        }
    }
}
