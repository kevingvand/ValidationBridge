using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValidationBridge.Invoker
{
    public class Modules
    {
        private static BridgeClient _client;

        private static BridgeClient GetClient()
        {
            if (_client == null)
            {
                _client = new BridgeClient();
            }
            return _client;
        }

        public static List<string> GetLoadedModules()
        {
            var client = GetClient();
            client.Stream.WriteString("modules");
            return client.Stream.ReadString().Split(',').ToList();
            //return null; //TODO: request from client
        }

        public static IModule GetModule(string name)
        {
            return null; //TODO: request from client
        }

        public static TModule GetModuleWithType<TModule>(string name)
        {
            return default(TModule); //TODO: get from client
        }
    }
}
