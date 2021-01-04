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
            : base()
        {
            LoadedModules = new List<IModule>
            {
                new Keithley2000() //TODO: instance should be created on the fly (multiple instances needed)
            };
        }

        public override Guid CreateModuleInstance(string name)
        {
            if (name.Equals("Keithley2000"))
            {
                var instance = new Keithley2000();
                return InstanceManager.CreateInstance(instance);
            }

            return Guid.Empty;
        }
    }
}
