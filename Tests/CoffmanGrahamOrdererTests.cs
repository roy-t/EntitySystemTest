using System;
using System.Collections.Generic;
using System.Threading;
using EntitySystemTest;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void LinearStages()
        {
            var specA = Spec().Produces("Product", "New");
            var specB = Spec().Requires("Product", "New")
                              .Produces("Product", "Improved");

            var specC = Spec().Requires("Product", "New")
                              .Requires("Product", "Improved");


            var specs = new List<SystemSpec>() { specC, specB, specA };
            specs.Shuffle();

            var ordered = CoffmanGrahamOrderer.Order(specs);

            Assert.That(ordered[0], Is.EqualTo(specA));
            Assert.That(ordered[1], Is.EqualTo(specB));
            Assert.That(ordered[2], Is.EqualTo(specC));

            var stages = CoffmanGrahamOrderer.DivideIntoStages(ordered);
            Assert.That(stages.Count, Is.EqualTo(3));
        }



        [Test]
        public void ParallelStages()
        {
            var specA = Spec().Produces("Product", "New");
            var specB = Spec().Requires("Product", "New")
                              .Parallel();
            var specC = Spec().Requires("Product", "New")
                              .Parallel();
            var specD = Spec().Requires("Product", "New")
                              .Parallel();

            var specs = new List<SystemSpec>() { specD, specC, specB, specA };
            specs.Shuffle();

            var ordered = CoffmanGrahamOrderer.Order(specs);
            var stages = CoffmanGrahamOrderer.DivideIntoStages(ordered);
            Assert.That(stages.Count, Is.EqualTo(2));

            Assert.That(stages[0].SystemSpecs.Count, Is.EqualTo(1));
            Assert.That(stages[0].SystemSpecs[0], Is.EqualTo(specA));

            Assert.That(stages[1].SystemSpecs.Count, Is.EqualTo(3));
        }

        private static SystemSpec Spec() => new SystemSpec(typeof(SoloSystem));
    }

    static class ListExtensions
    {
        [ThreadStatic] public static Random Random = new Random(unchecked((Environment.TickCount * 31) + Thread.CurrentThread.ManagedThreadId));

        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}