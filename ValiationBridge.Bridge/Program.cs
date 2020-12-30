using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Adapters;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Messages;

namespace ValiationBridge.Bridge
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: multiple Server instances 
            var server = new BridgeServer();
            var adapter = new CSharpAdapter();

            var instanceManager = new InstanceManager();

            server.WaitForConnection();


            Console.WriteLine("Client connected.");

            while(true)
            {
                var message = server.Stream.ReadString();

                if (message == null) return;

                if (message.Equals("modules"))
                    server.Stream.WriteString(string.Join(", ", adapter.LoadedModules.Select(x => x.Name)));
            }
                
        }
    }
}
