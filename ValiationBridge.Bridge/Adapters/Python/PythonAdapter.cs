using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Services;

namespace ValiationBridge.Bridge.Adapters.Python
{
    public class PythonAdapter : BaseAdapter
    {
        private LogService _logService;

        public PythonAdapter()
        {
            _logService = new LogService();
        }

        public override List<string> LoadModule(string modulePath)
        {


            return null;
        }
    }
}
