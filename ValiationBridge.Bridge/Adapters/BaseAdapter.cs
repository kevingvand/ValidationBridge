using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValiationBridge.Bridge.Adapters
{
    public abstract class BaseAdapter
    {
        public Dictionary<string, Type> LoadedModules { get; set; }

        public BaseAdapter()
        {
            LoadedModules = new Dictionary<string, Type>();
        }

        public virtual IModule GetModule(string name)
        {
            if (!LoadedModules.ContainsKey(name))
                return null;

            var moduleType = LoadedModules[name];
            return (IModule)Activator.CreateInstance(moduleType);
        }

        public abstract List<string> LoadModule(string modulePath);
    }
}
