using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EntitySystemTest;
using NUnit.Framework;

namespace Tests
{
    public class CoffmanGrahamOrdererTests
    {
        [Test]
        public void SequentialSystemSpecs()
        {
            var specA = Spec().Produces("Product", "New");
            var specB = Spec().Requires("Product", "New")
                              .Produces("Product", "Improved")
                              .InSequence();

            var specC = Spec().Requires("Product", "New")
                              .Requires("Product", "Improved")
                              .InSequence();


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
        public void ParallelSystemSpecs()
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


        [Test]
        public void MixedSystemSpecs()
        {
            var specA = Spec().Produces("Product", "New");

            var specB = Spec().Requires("Product", "New")
                              .Produces("Parallel", "1")
                              .Parallel();

            var specC = Spec().Requires("Product", "New")
                              .Produces("Sequential", "1")
                              .InSequence();

            var specD = Spec().Requires("Product", "New")
                              .Produces("Parallel", "2")
                              .Parallel();

            var specE = Spec().Requires("Product", "New")
                              .Produces("Sequential", "2")
                              .InSequence();

            var specF = Spec().Requires("Sequential", "1")
                              .Requires("Parallel", "1")
                              .Requires("Parallel", "2")
                              .Parallel();

            var specs = new List<SystemSpec>() { specF, specE, specD, specC, specB, specA };
            specs.Shuffle();

            var ordered = CoffmanGrahamOrderer.Order(specs);
            var stages = CoffmanGrahamOrderer.DivideIntoStages(ordered);
            Assert.That(stages.Count, Is.EqualTo(4));

            Assert.That(stages[0].SystemSpecs.Count, Is.EqualTo(1));
            Assert.That(stages[0].SystemSpecs[0], Is.EqualTo(specA));

            Assert.That(stages[1].SystemSpecs.Count, Is.EqualTo(2));
            Assert.That(stages[1].SystemSpecs.All(s => s.AllowParallelism), Is.EqualTo(true));

            Assert.That(stages[2].SystemSpecs.Count, Is.EqualTo(2));
            Assert.That(stages[2].SystemSpecs.All(s => s.AllowParallelism), Is.EqualTo(false));

            Assert.That(stages[3].SystemSpecs.Count, Is.EqualTo(1));
            Assert.That(stages[3].SystemSpecs[0], Is.EqualTo(specF));
        }
    }

}
internal static class ListExtensions
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
