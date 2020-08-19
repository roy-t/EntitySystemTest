using System;
using System.Collections.Generic;
using System.Linq;

namespace EntitySystemTest
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Coffman%E2%80%93Graham_algorithm
    /// </summary>
    public static class CoffmanGrahamOrderer
    {
        public static List<SystemSpec> Order(List<SystemSpec> systemSpecs)
        {
            var orderedSystemSpecs = new List<SystemSpec>();

            // First insert all items that do not have requirements
            for (var i = systemSpecs.Count - 1; i >= 0; i--)
            {
                var systemSpec = systemSpecs[i];
                if (systemSpec.RequiredResources.Count == 0)
                {
                    orderedSystemSpecs.Add(systemSpec);
                    systemSpecs.RemoveAt(i);
                }
            }

            // Keep inserting items for which all requirments have been fulfilled until the list is empty
            while (systemSpecs.Count > 0)
            {
                var candidate = GetNextCandidate(systemSpecs, orderedSystemSpecs);

                if (candidate == null)
                {
                    throw new Exception("Dependency cycle detected");
                }

                orderedSystemSpecs.Add(candidate);
                systemSpecs.Remove(candidate);
            }

            return orderedSystemSpecs;
        }

        public static List<Stage> DivideIntoStages(List<SystemSpec> orderedSystemSpecs)
        {
            var stages = new List<Stage>();
            var produced = new List<ResourceState>();

            var currentStage = new List<SystemSpec>();
            var allowParallelism = true;

            foreach (var systemSpec in orderedSystemSpecs)
            {
                if (AllRequirementsHaveBeenProduced(systemSpec, produced))
                {
                    if (currentStage.Count == 0)
                    {
                        currentStage.Add(systemSpec);
                        allowParallelism = systemSpec.AllowParallelism;
                    }
                    else if (currentStage.Count > 0 && systemSpec.AllowParallelism && allowParallelism)
                    {
                        currentStage.Add(systemSpec);
                    }
                    else
                    {
                        stages.Add(new Stage(currentStage));
                        currentStage = new List<SystemSpec>() { systemSpec };
                        allowParallelism = systemSpec.AllowParallelism;
                    }
                }
                else
                {
                    produced.AddRange(GetProducedResource(currentStage));
                    if (AllRequirementsHaveBeenProduced(systemSpec, produced))
                    {
                        stages.Add(new Stage(currentStage));
                        currentStage = new List<SystemSpec>() { systemSpec };
                        allowParallelism = systemSpec.AllowParallelism;
                    }
                    else
                    {
                        throw new Exception("Algorithm error");
                    }
                }
            }

            if (currentStage.Count > 0)
            {
                stages.Add(new Stage(currentStage));
            }

            stages.TrimExcess();
            return stages;
        }

        private static bool AllRequirementsHaveBeenProduced(SystemSpec systemSpec, List<ResourceState> produced)
        {
            foreach (var requirement in systemSpec.RequiredResources)
            {
                if (!produced.Contains(requirement))
                {
                    return false;
                }
            }

            return true;
        }

        private static List<ResourceState> GetProducedResource(List<SystemSpec> systemSpecs)
        {
            var produced = new List<ResourceState>();
            foreach (var systemSpec in systemSpecs)
            {
                produced.AddRange(systemSpec.ProducedResources);
            }

            return produced;
        }

        private static SystemSpec GetNextCandidate(List<SystemSpec> systemSpecs, List<SystemSpec> orderedSystemSpecs)
        {
            var maxDistance = int.MinValue;
            var preferedParallelness = orderedSystemSpecs.LastOrDefault()?.AllowParallelism ?? true;
            SystemSpec candidate = null;


            foreach (var systemSpec in systemSpecs)
            {
                // The best candidate has the largest distance from the items that produce their requirements
                // so that it does not have to wait long for what it needs.
                // As a tie breaker we use the if the parallelism is the same as the last added item in the hope
                // to get larger consecutive items that can be run in parallel.
                var minDistance = int.MaxValue;
                foreach (var requirement in systemSpec.RequiredResources)
                {
                    var distance = DistanceTo(requirement, orderedSystemSpecs);
                    if (distance == null)
                    {
                        goto NextCandidate;
                    }

                    minDistance = Math.Min(distance.Value, minDistance);
                }

                if (minDistance > maxDistance ||
                    (minDistance >= maxDistance && systemSpec.AllowParallelism == preferedParallelness))
                {
                    maxDistance = minDistance;
                    candidate = systemSpec;
                }

            NextCandidate: { }
            }

            return candidate;
        }


        private static int? DistanceTo(ResourceState requirement, List<SystemSpec> orderedSystemSpecs)
        {
            var producer = orderedSystemSpecs.Where(systemSpec => systemSpec.ProducedResources.Contains(requirement)).FirstOrDefault();

            if (producer != null)
            {
                var insertAt = orderedSystemSpecs.Count;
                return insertAt - orderedSystemSpecs.IndexOf(producer);
            }

            return null;
        }
    }
}
