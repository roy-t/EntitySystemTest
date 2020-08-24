using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EntitySystemTest.Pipeline
{
    public sealed class ParallelPipeline : IDisposable
    {
        private readonly int MaxConcurrency;
        private readonly Thread[] Threads;
        private readonly CountdownEvent[] StageCountDownEvents;
        private readonly CountdownEvent FrameStartCountDownEvent;

        public ParallelPipeline(IReadOnlyList<PipelineStage> pipelineStages)
        {
            this.PipelineStages = pipelineStages;
            this.MaxConcurrency = this.PipelineStages.Max(stage => stage.Systems.Count);

            this.StageCountDownEvents = new CountdownEvent[this.PipelineStages.Count];
            for (var i = 0; i < this.PipelineStages.Count; i++)
            {
                this.StageCountDownEvents[i] = new CountdownEvent(this.MaxConcurrency);
            }
            this.FrameStartCountDownEvent = new CountdownEvent(1);

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
        public int ActiveThreads => this.Threads.Sum(x => x.IsAlive ? 1 : 0);


        public void NextFrame()
        {
            for (var i = this.PipelineStages.Count - 1; i >= 0; i--)
            {
                this.StageCountDownEvents[i].Reset();
            }

            this.FrameStartCountDownEvent.Signal();
        }

        public void WaitForEndOfFrame()
        {
            if (this.IsRunning)
            {
                this.StageCountDownEvents[^1].Wait();
            }
        }

        public void Stop()
        {
            if (this.IsRunning)
            {
                this.WaitForEndOfFrame();
                this.IsRunning = false; // What if someone else already started the next frame? Should I sync all methods?
                this.NextFrame();

                for (var i = 0; i < this.MaxConcurrency; i++)
                {
                    this.Threads[i].Join();
                }
            }
        }

        public void Dispose()
        {
            if (this.IsRunning)
            {
                throw new InvalidOperationException("Cannot dispose while still running");
            }

            for (var i = 0; i < this.StageCountDownEvents.Length; i++)
            {
                this.StageCountDownEvents[i]?.Dispose();
            }
            this.FrameStartCountDownEvent?.Dispose();
        }

        private void ThreadStart(object threadIndexObj)
        {
            var threadIndex = (int)threadIndexObj;
            while (true)
            {
                this.FrameStartCountDownEvent.Wait();
                if (threadIndex == 0)
                {
                    this.FrameStartCountDownEvent.Reset();
                }

                if (!this.IsRunning)
                {
                    break;
                }

                for (var currentStage = 0; currentStage < this.PipelineStages.Count; currentStage++)
                {
                    var stage = this.PipelineStages[currentStage];
                    if (threadIndex < stage.Systems.Count)
                    {
                        var system = stage.Systems[threadIndex];
                        system.Process();
                    }

                    this.StageCountDownEvents[currentStage].Signal();
                    this.StageCountDownEvents[currentStage].Wait();
                }

                while (this.FrameStartCountDownEvent.IsSet) { /* Wait for thread 0 to reset the CountDownEvent */ };
            }
        }
    }
}
