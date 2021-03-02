using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Bridge.Adapters;
using ValidationBridge.Bridge.Adapters.Python;
using ValidationBridge.Bridge.Services;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Messages;

namespace ValidationBridge.Bridge
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: multiple bridge instances 
            var bridge = new Bridge();
            bridge.Start();
        }
    }
}
