using System;
using System.Collections.Generic;
using System.Reflection;
using PlainDictionary = System.Collections.IDictionary;

namespace EntitySystemTest
{
    public sealed class EarlyPipeline
    {
        private readonly Dictionary<Type, PlainDictionary> ComponentLookUps;
        private readonly List<SystemBinding> SystemBindings;

        public EarlyPipeline()
        {
            this.ComponentLookUps = new Dictionary<Type, PlainDictionary>();
            this.SystemBindings = new List<SystemBinding>();
        }

        public void AddSystem(ISystem system)
        {
            var type = system.GetType();
            var interfaces = type.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                if (typeof(ISystem).IsAssignableFrom(@interface) && @interface != typeof(ISystem))
                {
                    var arguments = @interface.GetGenericArguments();
                    var parameterLookups = new PlainDictionary[arguments.Length];
                    for (var i = 0; i < arguments.Length; i++)
                    {
                        parameterLookups[i] = this.ComponentLookUps[arguments[i]];
                    }

                    var processor = type.GetMethod("Process");

                    //var delegateType = Expression.GetActionType(arguments);
                    //var processDelegate = processor.CreateDelegate(delegateType, system);
                    this.SystemBindings.Add(new SystemBinding(processor, system, parameterLookups));
                }
            }
        }

        public void AddComponentLookUp<T>(Dictionary<int, T> componentLookUp)
             where T : IComponent
             => this.ComponentLookUps.Add(typeof(T), componentLookUp);
        internal void Execute()
        {
            foreach (var systemBinding in this.SystemBindings)
            {
                systemBinding.Process();
            }
        }
    }



    public sealed class SystemBinding
    {
        public SystemBinding(MethodInfo processDelegate, ISystem system, IReadOnlyList<PlainDictionary> lookUps)
        {
            this.ProcessDelegate = processDelegate;
            this.System = system;
            this.LookUps = lookUps;
        }

        public MethodInfo ProcessDelegate { get; }
        public ISystem System { get; }
        public IReadOnlyList<PlainDictionary> LookUps { get; }

        public void Process()
        {
            var parameters = new object[this.LookUps.Count]; // can we make this IComponent?

            foreach (var primaryComponent in this.LookUps[0].Values)
            {
                parameters[0] = primaryComponent;

                var satisfied = true;
                if (parameters.Length > 1)
                {
                    var entity = ((IComponent)primaryComponent).Entity;
                    for (var i = 1; i < parameters.Length; i++)
                    {
                        var lookup = this.LookUps[i];
                        if (lookup.Contains(entity))
                        {
                            parameters[i] = lookup[i];
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
                    this.ProcessDelegate.Invoke(this.System, parameters);
                }

            }
        }
    }
}
