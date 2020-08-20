using System.Collections.Generic;
using EntitySystemTest.Mocks;
using EntitySystemTest.Pipeline;
using LightInject;
using NUnit.Framework;

namespace Tests
{
    public class PipelineBuilderTests
    {
        [Test]
        public void Integration()
        {
            var builder = new PipelineBuilder();

            // Stage 0
            builder.AddSystem<SoloSystem>()
                .Produces("GBuffer", "Cleared")
                .InSequence();

            // Stage 1
            builder.AddSystem<MonoSystem>()
                .Requires("GBuffer", "Cleared")
                .Produces("GBuffer", "Materials")
                .InSequence();

            // Stage 2
            builder.AddSystem<DuoSystem>()
                .Requires("GBuffer", "Materials")
                .Produces("GBuffer", "Point-Lights")
                .InSequence();

            // Stage 3
            builder.AddSystem<MonoSystem>()
                .Requires("GBuffer", "Point-Lights")
                .Produces("Foo", "Bar")
                .Parallel();

            builder.AddSystem<MonoSystem>()
                .Requires("GBuffer", "Point-Lights")
                .Produces("Foo", "Bar2")
                .Parallel();

            // Stage 4
            builder.AddSystem<MonoSystem>()
                   .Requires("GBuffer", "Cleared")
                   .Requires("Foo", "Bar")
                   .Produces("Zzz", "yyy")
                   .Parallel();

            // Stage 5
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

            var serviceContainer = new ServiceContainer(ContainerOptions.Default);
            serviceContainer.SetDefaultLifetime<PerContainerLifetime>();

            serviceContainer.Register<SoloSystem>();
            serviceContainer.Register<MonoSystem>();
            serviceContainer.Register<DuoSystem>();

            var parallelPipeline = builder.Build(serviceContainer);

            Assert.That(parallelPipeline.PipelineStages.Count, Is.EqualTo(6));
        }
    }
}
