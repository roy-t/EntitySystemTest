using System;
using System.Collections.Generic;
using PlainDictionary = System.Collections.IDictionary;

namespace EntitySystemTest.Systems
{
    public static class SystemBinder
    {
        public static List<ISystemBinding> BindSystem(ISystem system, Dictionary<Type, PlainDictionary> componentContainers)
        {
            var systemBindings = new List<ISystemBinding>();
            var type = system.GetType();
            var interfaces = type.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                if (typeof(ISystem).IsAssignableFrom(@interface) && @interface != typeof(ISystem))
                {
                    var processor = type.GetMethod("Process");

                    var arguments = @interface.GetGenericArguments();

                    if (arguments.Length == 0)
                    {
                        systemBindings.Add(new SystemBindingWithoutComponents(processor, system));
                    }
                    else if (arguments.Length == 1)
                    {
                        var parameterLookup = componentContainers[arguments[0]];
                        systemBindings.Add(new SystemBindingWithOneComponent(processor, system, parameterLookup));
                    }
                    else
                    {
                        var parameterLookups = new PlainDictionary[arguments.Length];
                        for (var i = 0; i < arguments.Length; i++)
                        {
                            parameterLookups[i] = componentContainers[arguments[i]];
                        }
                        systemBindings.Add(new SystemBindingWithManyComponents(processor, system, parameterLookups));
                    }

                }
            }

            return systemBindings;
        }
    }
}
