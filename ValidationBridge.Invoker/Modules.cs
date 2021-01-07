using System;
using System.Collections.Generic;
using System.Linq;
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

            if (result.MessageType == EMessageType.ERROR) return new List<string>(); //TODO error handling.

            var resultMessage = (ResultMessage)result;

            return ((string[])resultMessage.Result.Value).ToList();
        }

        public static List<string> LoadModule(string modulePath)
        {
            var client = GetClient();

            var message = new InvokeMessage(Constants.Commands.LoadModule, new Argument(modulePath));
            var result = client.WriteMessage(message);

            if (result.MessageType == EMessageType.ERROR) return new List<string>(); //TODO error handling.
            var resultMessage = (ResultMessage)result;

            return ((string[])resultMessage.Result.Value).ToList();
        }

        public static TModule Cast<TModule>(IModule module)
        {
            return Cast<TModule>(module.InstanceId);
        }

        public static TModule Cast<TModule>(Guid instanceId)
        {
            var client = GetClient();

            var message = new InvokeMessage(Constants.Commands.GetModuleInterfaces, new Argument(instanceId));
            var result = client.WriteMessage(message);

            if (result.MessageType == EMessageType.ERROR) return default(TModule); //TODO: better error handling

            var resultMessage = (ResultMessage)result;

            var interfaceTypes = resultMessage.Result.GetValue<string[]>();
            if (!interfaceTypes.Contains(typeof(TModule).AssemblyQualifiedName))
                return default(TModule); //TODO: better error handling

            var proxy = new ModuleProxy<TModule>(instanceId);
            proxy.CreateProxy(client);
            return proxy.GetModule();
        }

        public static IModule GetModule(string name)
        {
            return GetModuleWithType<IModule>(name);
        }

        public static TModule GetModuleWithType<TModule>(string name)
        {
            var client = GetClient();
            var message = new InvokeMessage(Constants.Commands.GetModule, new Argument(name), new Argument(typeof(TModule).AssemblyQualifiedName));
            var result = client.WriteMessage(message);

            if (result.MessageType == EMessageType.ERROR) return default(TModule); //TODO error handling.
            var resultMessage = (ResultMessage)result;

            //TODO: pass interface and make sure module implements interface

            var instanceId = resultMessage.Result.GetValue<Guid>();

            if (resultMessage.Result.Type != EType.HANDLE || resultMessage.Result.GetValue<Guid>() == Guid.Empty)
            {
                return default(TModule);
                //TODO: better error handling
                //throw new Exception("Could not create instance of module.");
            }

            return Cast<TModule>(instanceId);
        }
    }
}
