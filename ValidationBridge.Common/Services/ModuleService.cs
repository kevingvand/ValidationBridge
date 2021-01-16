using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValidationBridge.Common.Services
{
    public class ModuleService
    {
        private static ICollection<Type> _commonModuleTypes;
        private static ICollection<Type> GetCommonModuleTypes()
        {
            if (_commonModuleTypes == null)
            {
                var commonAssembly = Assembly.GetAssembly(typeof(IModule));
                _commonModuleTypes = commonAssembly.GetTypes().Where(type => type.GetInterface(typeof(IModule).FullName) != null).ToList();
                _commonModuleTypes.Add(typeof(IModule));
            }

            return _commonModuleTypes;
        }

        public static Type GetModuleType(string typeName)
        {
            var types = GetCommonModuleTypes();

            var typeFromAssemblyQualifiedName = types.FirstOrDefault(type => type.AssemblyQualifiedName.Equals(typeName));
            if (typeFromAssemblyQualifiedName != null) return typeFromAssemblyQualifiedName;

            var typeFromFullName = types.FirstOrDefault(type => type.FullName.Equals(typeName));
            if (typeFromFullName != null) return typeFromFullName;

            var typeFromName = types.FirstOrDefault(type => type.Name.Equals(typeName));
            if (typeFromName != null) return typeFromName;

            return null;
        }

        public static IEnumerable<PropertyInfo> GetPublicProperties(Type type)
        {
            if (!type.IsInterface)
                return type.GetProperties();

            return (new Type[] { type })
                   .Concat(type.GetInterfaces())
                   .SelectMany(i => i.GetProperties());
        }

        public static IEnumerable<MethodInfo> GetPublicMethods(Type type)
        {
            if (!type.IsInterface)
                return type.GetMethods();

            return (new Type[] { type })
                   .Concat(type.GetInterfaces())
                   .SelectMany(i => i.GetMethods());
        }
    }
}
