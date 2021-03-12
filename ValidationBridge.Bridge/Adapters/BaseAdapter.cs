using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValidationBridge.Bridge.Adapters
{
    public abstract class BaseAdapter : IModuleAdapter
    {
        public Dictionary<string, Type> LoadedModules { get; set; }

        public BaseAdapter()
        {
            LoadedModules = new Dictionary<string, Type>();
        }

        public virtual IModule GetModule(string name)
        {
            // Check if a module with the specified name exists
            if (!LoadedModules.ContainsKey(name))
                return null;

            // If a type was found, a new instance is created
            var moduleType = LoadedModules[name];
            return (IModule)Activator.CreateInstance(moduleType);
        }

        public Dictionary<string, Type> LoadModules(string modulePath)
        {
            //TODO: replace LoadModule with this method.
            throw new NotImplementedException();
        }

        public abstract List<string> LoadModule(string modulePath);
    }
}
