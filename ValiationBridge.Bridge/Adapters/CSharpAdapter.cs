using InstrumentModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValiationBridge.Bridge.Adapters
{
    public class CSharpAdapter : BaseAdapter
    {
        public CSharpAdapter()
        {
            LoadedModules = new List<IModule>();
            LoadedModules.Add(new Keithley2000()); //TODO: instance should be created on the fly (multiple instances needed)
        }
    }
}
