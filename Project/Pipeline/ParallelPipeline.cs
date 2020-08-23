using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EntitySystemTest.Pipeline
{
    public sealed class ParallelPipeline
    {
        private readonly int MaxConcurrency;
        private readonly Thread[] Threads;
        private readonly CountdownEvent[] StageCountDownEvents;
        private readonly CountdownEvent[] FrameCountDownEvents;

        private int currentFrame;
        private int currentStage;

        public ParallelPipeline(IReadOnlyList<PipelineStage> pipelineStages)
        {
            this.PipelineStages = pipelineStages;
            for (var i = 0; i < this.PipelineStages.Count; i++)
            {
                var stage = this.PipelineStages[i];
                this.MaxConcurrency = Math.Max(stage.Systems.Count, this.MaxConcurrency);
            }

            this.StageCountDownEvents = new CountdownEvent[this.PipelineStages.Count];
            for (var i = 0; i < this.PipelineStages.Count; i++)
            {
                this.StageCountDownEvents[i] = new CountdownEvent(this.MaxConcurrency);
            }

            this.FrameCountDownEvents = new CountdownEvent[]
            {
                new CountdownEvent(this.MaxConcurrency),
                new CountdownEvent(this.MaxConcurrency)
            };

            this.IsRunning = true;

            this.Threads = new Thread[this.MaxConcurrency];
            for (var i = 0; i < this.MaxConcurrency; i++)
            {
                this.Threads[i] = new Thread(threadIndex => this.ThreadStart(threadIndex));
                this.Threads[i].Start(i);
            }
        }

        public IReadOnlyList<PipelineStage> PipelineStages { get; }

        public bool IsRunning { get; private set; }

        public void Stop()
        {
            // What if, NextFrame was never called?

            this.WaitForEndOfFrame();
            this.IsRunning = false;
            this.NextFrame();

            for (var i = 0; i < this.MaxConcurrency; i++)
            {
                this.Threads[i].Join();
            }
        }

        public void NextFrame()
        {
            // TODO: what if someone calls NextFrame at random moments?
            this.NextFrameCountDownEvent.Reset();
            for (var i = this.PipelineStages.Count - 1; i >= 0; i--)
            {
                this.StageCountDownEvents[i].Reset();
            }

            this.CurrentFrameCountDownEvent.Signal(this.MaxConcurrency);
        }

        public void WaitForEndOfFrame()
        {
            if (this.IsRunning)
            {
                this.StageCountDownEvents[this.PipelineStages.Count - 1].Wait();
            }
        }

        public int ActiveThreads => this.Threads.Sum(x => x.IsAlive ? 1 : 0);

        private void ThreadStart(object threadIndexObj)
        {
            var threadIndex = (int)threadIndexObj;
            while (true)
            {
                var currentFrame = this.currentFrame;
                this.CurrentFrameCountDownEvent.Wait();

                if (!this.IsRunning)
                {
                    break;
                }

                for (this.currentStage = 0; this.currentStage < this.PipelineStages.Count; this.currentStage++)
                {
                    var stage = this.PipelineStages[this.currentStage];
                    if (threadIndex < stage.Systems.Count)
                    {
                        var system = stage.Systems[threadIndex];
                        system.Process();
                    }

                    this.StageCountDownEvents[this.currentStage].Signal();
                    this.StageCountDownEvents[this.currentStage].Wait();
                }

                this.currentFrame = currentFrame + 1; // All threads will write the same value to this field
            }
        }

        private CountdownEvent CurrentFrameCountDownEvent => this.FrameCountDownEvents[this.currentFrame % 2 == 0 ? 0 : 1];
        private CountdownEvent NextFrameCountDownEvent => this.FrameCountDownEvents[this.currentFrame % 2 == 0 ? 1 : 0];
    }
}
