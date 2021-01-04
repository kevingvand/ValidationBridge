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
        public List<IModule> LoadedModules { get; set; }
        public InstanceManager InstanceManager { get; set; }

        public BaseAdapter()
        {
            LoadedModules = new List<IModule>();
            InstanceManager = new InstanceManager();
        }

        public IModule GetModule(Guid instanceId)
        {
            if (!InstanceManager.Instances.ContainsKey(instanceId)) return null;
            return (IModule) InstanceManager.Instances[instanceId].Instance;
        }

        public TModule GetModule<TModule>(Guid instanceId) where TModule : IModule
        {
            var instance = GetModule(instanceId);
            if (instance == null) return default(TModule);

            return (TModule)instance; //TODO: check if module implements interface
        }

        public abstract Guid CreateModuleInstance(string name);

    }
}
