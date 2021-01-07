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

        public BaseAdapter()
        {
            LoadedModules = new List<IModule>();
        }

        public abstract IModule GetModule(string name);

        public abstract List<string> LoadModule(string modulePath);
    }
}
