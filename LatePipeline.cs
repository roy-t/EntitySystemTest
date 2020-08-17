using System;
using System.Collections;
using System.Collections.Generic;
#if RELEASE
#endif
namespace EntitySystemTest
{
    public class LatePipeline
    {
        private readonly Dictionary<Type, IDictionary> ComponentLookUps;
        private readonly List<ISystem> Systems;

        public LatePipeline()
        {
            this.ComponentLookUps = new Dictionary<Type, IDictionary>();
            this.Systems = new List<ISystem>();
        }

        public void AddSystem(ISystem system)
            => this.Systems.Add(system);

        public void AddComponentLookUp<T>(Dictionary<int, T> componentLookUp)
            where T : IComponent
            => this.ComponentLookUps.Add(typeof(T), componentLookUp);
        internal void Execute()
        {
            foreach (var system in this.Systems)
            {
                var systemType = system.GetType();
                var interfaces = systemType.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    if (typeof(ISystem).IsAssignableFrom(@interface) && @interface != typeof(ISystem))
                    {
                        var arguments = @interface.GetGenericArguments();
                        var parameterLookups = new IDictionary[arguments.Length];

                        for (var i = 0; i < arguments.Length; i++)
                        {
                            parameterLookups[i] = this.ComponentLookUps[arguments[i]];
                        }


                        var processor = systemType.GetMethod("Process");
                        foreach (var component in parameterLookups[0].Values)
                        {
                            var parameters = new object[arguments.Length];
                            parameters[0] = component;

                            var satisfied = true;
                            if (parameters.Length > 1)
                            {
                                var entity = ((IComponent)component).Entity;
                                for (var i = 1; i < parameterLookups.Length; i++)
                                {
                                    var lookUp = parameterLookups[i];
                                    if (lookUp.Contains(entity))
                                    {
                                        parameters[i] = lookUp[entity];
                                    }
                                    else
                                    {
                                        satisfied = false;
                                        break;
                                    }
                                }
                            }

                            if (satisfied)
                            {
                                processor.Invoke(system, parameters);
                            }
                        }
                    }
                }
            }
        }
    }
}
