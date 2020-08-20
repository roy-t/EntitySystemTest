using System;
using System.Collections.Generic;
using EntitySystemTest.Mocks;
using EntitySystemTest.Systems;
using NUnit.Framework;
using PlainDictionary = System.Collections.IDictionary;

namespace Tests
{
    public class SystemBinderTests
    {
        [Test]
        public void SystemWithoutComponent()
        {
            var componentContainers = new Dictionary<Type, PlainDictionary>();
            var bindings = SystemBinder.BindSystem(new SoloSystem(), componentContainers);

            Assert.That(bindings.Count, Is.EqualTo(1));
            Assert.That(bindings[0], Is.TypeOf<SystemBindingWithoutComponents>());
        }

        [Test]
        public void SystemWithOneComponent()
        {
            var componentContainers = new Dictionary<Type, PlainDictionary>
            {
                { typeof(ComponentA), new Dictionary<int, ComponentA>() }
            };

            var bindings = SystemBinder.BindSystem(new MonoSystem(), componentContainers);

            Assert.That(bindings.Count, Is.EqualTo(1));
            Assert.That(bindings[0], Is.TypeOf<SystemBindingWithOneComponent>());
        }

        [Test]
        public void SystemWithManyComponents()
        {
            var componentContainers = new Dictionary<Type, PlainDictionary>
            {
                { typeof(ComponentA), new Dictionary<int, ComponentA>() },
                { typeof(ComponentB), new Dictionary<int, ComponentB>() }
            };

            var bindings = SystemBinder.BindSystem(new DuoSystem(), componentContainers);

            Assert.That(bindings.Count, Is.EqualTo(1));
            Assert.That(bindings[0], Is.TypeOf<SystemBindingWithManyComponents>());
        }
    }
}
