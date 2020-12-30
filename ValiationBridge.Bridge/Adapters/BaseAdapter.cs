using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValiationBridge.Bridge.Adapters
{
    public class BaseAdapter
    {
        public ICollection<IModule> LoadedModules { get; set; }
    }
}
