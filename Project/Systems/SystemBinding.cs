using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlainDictionary = System.Collections.IDictionary;

namespace EntitySystemTest.Systems
{
    public interface ISystemBinding
    {
        public void Process();
    }

    public class SystemBindingWithoutComponents : ISystemBinding
    {
        private readonly MethodInfo ProcessDelegate;
        private readonly ISystem System;

        public SystemBindingWithoutComponents(MethodInfo processDelegate, ISystem system)
        {
            this.ProcessDelegate = processDelegate;
            this.System = system;
        }

        public void Process()
            => this.ProcessDelegate.Invoke(this.System, null);

        public override string ToString()
            => $"{this.System.GetType().Name}<>";
    }

    public class SystemBindingWithOneComponent : ISystemBinding
    {
        private readonly MethodInfo ProcessDelegate;
        private readonly ISystem System;
        private readonly PlainDictionary ComponentContainer;
        private readonly object[] Parameters;

        public SystemBindingWithOneComponent(MethodInfo processDelegate, ISystem system, PlainDictionary componentContainer)
        {
            this.ProcessDelegate = processDelegate;
            this.System = system;
            this.ComponentContainer = componentContainer;
            this.Parameters = new object[1];
        }

        public void Process()
        {
            foreach (var component in this.ComponentContainer.Values)
            {
                this.Parameters[0] = component;
                this.ProcessDelegate.Invoke(this.System, this.Parameters);
            }
        }

        public override string ToString()
            => $"{this.System.GetType().Name}<{this.ComponentContainer.GetType().GenericTypeArguments[1].Name}>";
    }

    public class SystemBindingWithManyComponents : ISystemBinding
    {
        private readonly MethodInfo ProcessDelegate;
        private readonly ISystem System;
        private readonly IReadOnlyList<PlainDictionary> ComponentContainers;
        private readonly object[] Parameters;

        public SystemBindingWithManyComponents(MethodInfo processDelegate, ISystem system, IReadOnlyList<PlainDictionary> componentContainers)
        {
            this.ProcessDelegate = processDelegate;
            this.System = system;
            this.ComponentContainers = componentContainers;
            this.Parameters = new object[componentContainers.Count];
        }

        public void Process()
        {
            foreach (var primaryComponent in this.ComponentContainers[0].Values)
            {
                this.Parameters[0] = primaryComponent;
                var entity = ((IComponent)primaryComponent).Entity;
                for (var i = 1; i < this.Parameters.Length; i++)
                {
                    var componentContainer = this.ComponentContainers[i];
                    this.Parameters[i] = componentContainer[entity];

                }

                this.ProcessDelegate.Invoke(this.System, this.Parameters);
            }
        }

        public override string ToString()
            => $"{this.System.GetType().Name}<{string.Join(", ", this.ComponentContainers.Select(c => c.GetType().GenericTypeArguments[1].Name))}>";
    }
}
