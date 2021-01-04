using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Interfaces.Modules;
using ValidationBridge.Common.Messages;
using ValidationBridge.Invoker.Proxy;

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
            var client = GetClient();
            var message = new InvokeMessage(Constants.Commands.GetModule, new Argument(name), new Argument(typeof(TModule).FullName));
            var resultMessage = client.WriteMessage(message);

            //TODO: pass interface and make sure module implements interface

            var instanceId = resultMessage.Result.GetValue<Guid>();

            if (resultMessage.Result.Type != EType.HANDLE || resultMessage.Result.GetValue<Guid>() == Guid.Empty)
            {
                //TODO: better error handling
                throw new Exception("Could not create instance of module.");
            }

            var proxy = new ModuleProxy();
            proxy.CreateProxy(typeof(TModule), client, instanceId);

            //TODO: possibility to retrieve error message

            return proxy.GetModule(typeof(TModule));
        }
    }
}
