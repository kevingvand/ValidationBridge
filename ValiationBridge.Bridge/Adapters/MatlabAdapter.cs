using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValiationBridge.Bridge.Adapters
{
    public class MatlabAdapter : BaseAdapter
    {
        public override IModule GetModule(string name)
        {
            throw new NotImplementedException();
        }

        public override List<string> LoadModule(string modulePath)
        {
            throw new NotImplementedException();
        }
    }
}
