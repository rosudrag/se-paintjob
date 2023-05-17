using System;
using System.Collections.Generic;

namespace PaintJob
{
    public static class SimpleIoC
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<TI, TD>(TD service) where TD : TI
        {
            var type = typeof(TI);
            if (_services.ContainsKey(type))
            {
                throw new Exception($"Service of type {type.Name} is already registered.");
            }

            _services[type] = service;
        }
        
        public static void Register<TI>(TI service)
        {
            var type = typeof(TI);
            if (_services.ContainsKey(type))
            {
                throw new Exception($"Service of type {type.Name} is already registered.");
            }

            _services[type] = service;
        }

        public static T Resolve<T>() where T : class
        {
            var type = typeof(T);
            if (!_services.TryGetValue(type, out var service))
            {
                throw new Exception($"Service of type {type.Name} is not registered.");
            }

            return service as T;
        }
    }
}