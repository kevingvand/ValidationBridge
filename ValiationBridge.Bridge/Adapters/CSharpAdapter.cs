using InstrumentModules;
using System;
using System.Collections.Generic;
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

        public override IModule GetModule(string name)
        {
            if (name.Equals("Keithley2000"))
            {
                return new Keithley2000();
            }
            return null;
        }
    }
}
