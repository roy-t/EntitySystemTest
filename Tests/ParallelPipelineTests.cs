using System.Collections.Generic;
using EntitySystemTest.Mocks;
using EntitySystemTest.Pipeline;
using EntitySystemTest.Systems;
using NUnit.Framework;

namespace Tests
{
    public class ParallelPipelineTests
    {
        [Test]
        public void Foo()
        {
            var pipelineStages = new List<PipelineStage>();

            var soloSystem = new SoloSystem();
            var methodInfo = soloSystem.GetType().GetMethod("Process");

            var systemBindings = new List<ISystemBinding>();
            systemBindings.Add(new SystemBindingWithoutComponents(methodInfo, soloSystem));
            pipelineStages.Add(new PipelineStage(systemBindings));
            pipelineStages.Add(new PipelineStage(systemBindings));

            var pipeline = new ParallelPipeline(pipelineStages);

            pipeline.Run();
            pipeline.Wait();
            Assert.That(pipeline.ActiveThreads, Is.EqualTo(1));

            pipeline.Run();
            pipeline.Wait();
            Assert.That(pipeline.ActiveThreads, Is.EqualTo(1));

            pipeline.Stop();
            Assert.That(pipeline.ActiveThreads, Is.EqualTo(0));
        }
    }
}
