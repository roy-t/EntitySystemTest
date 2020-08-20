using System.Collections.Generic;

namespace EntitySystemTest.Pipeline
{
    public sealed class ParallelPipeline
    {
        public ParallelPipeline(IReadOnlyList<PipelineStage> pipelineStages)
        {
            this.PipelineStages = pipelineStages;
        }

        public IReadOnlyList<PipelineStage> PipelineStages { get; }
    }
}
