using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common;
using ValidationBridge.Common.Interfaces.Modules;
using ValidationBridge.Common.Messages;

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

            var message = new InvokeMessage(Constants.Commands.GetModules);
            var result = client.WriteMessage(message);

            return ((string[])result.Result.Value).ToList();
        }

        public static IModule GetModule(string name)
        {


            return null; //TODO: request from client
        }

        public static TModule GetModuleWithType<TModule>(string name)
        {
            /*
                * 1. Send GetModuleRequest to  Bridge
                * 2. Pack in Proxy
                * 3. Return Proxy element
            */

            var client = GetClient();
            var message = new InvokeMessage(Constants.Commands.GetModule, new Argument(name), new Argument(typeof(TModule).FullName));
            var result = client.WriteMessage(message);

            var instanceId = (Guid)result.Result.Value;

            var invokeMessage = new InvokeMessage(instanceId, "GetACVoltage");
            var x = client.WriteMessage(invokeMessage);

            /*
             * Use Guid to make proxy --> call methods using the GUID
             */

            //TODO: pass interface and make sure module implements interface
            //TODO: make proxy for handle
            //TODO: possibility to retrieve error message

            return default(TModule); //TODO: get from client
        }
    }
}
