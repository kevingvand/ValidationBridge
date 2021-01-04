using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Adapters;
using ValiationBridge.Bridge.Services;
using ValidationBridge.Common;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Interfaces.Modules;
using ValidationBridge.Common.Messages;

namespace ValiationBridge.Bridge.Handlers
{
    public class GlobalInvocationHandler : MessageHandler
    {
        private LogService _logService;
        private InstanceManager _instanceManager;
        private List<BaseAdapter> _adapters;

        public GlobalInvocationHandler(InstanceManager instanceManager, List<BaseAdapter> adapters)
        {
            _logService = new LogService();
            _instanceManager = instanceManager;
            _adapters = adapters;
        }

        public override PipeMessage HandleMessages(PipeMessage message)
        {
            if (!(message is InvokeMessage invokeMessage)) return null;
            if (invokeMessage.InstanceId != Guid.Empty) return null;

            switch(invokeMessage.MethodName)
            {
                case Constants.Commands.GetModules:
                    return GetModules();
                case Constants.Commands.GetModule:
                    return GetModule(invokeMessage.Arguments);
                default:
                    return null;
            }
        }

        private ResultMessage GetModules()
        {
            var loadedModules = _adapters
                .SelectMany(adapter => adapter.LoadedModules)
                .Select(adapter => adapter.GetName()).ToArray();

            return new ResultMessage(new Argument(loadedModules));
        }

        //TODO: split into 2: Get module (retrieves an already created module) and Create Module (creates a new instance of a module)
        private ResultMessage GetModule(Argument[] arguments)
        {
            if(arguments.Length < 1)
            {
                _logService.LogError("Unable to retrieve module, no name specified");
                return null;
            }

            var nameArgument = arguments[0];
            if(nameArgument.Type != EType.STRING)
            {
                _logService.LogError($"Unable to retrieve module, name specified but in wrong format (got {nameArgument.Type}), expected {EType.STRING}).");
                return null;
            }

            //TODO: replace once real adapter is in place.
            //var loadedModules = _adapters.SelectMany(adapter => adapter.LoadedModules);
            //var module = loadedModules.FirstOrDefault(adapter => adapter.Name.Equals(nameArgument.GetValue<string>()));

            BaseAdapter moduleAdapter = _adapters.FirstOrDefault(adapter => adapter.LoadedModules.Any(loadedModule => loadedModule.GetName().Equals(nameArgument.GetValue<string>())));

            if(moduleAdapter == null)
            {
                _logService.LogError($"Unable to retrieve module, module with name {nameArgument.GetValue<string>()} not found.");
                return null;
            }

            var moduleInstance = moduleAdapter.GetModule(nameArgument.GetValue<string>());
            var instanceId = _instanceManager.CreateInstance(moduleInstance);
            return new ResultMessage(new Argument(instanceId));

            //TODO: check for 2nd argument (type) (if specified, else just return IModule?)
            //var a = module is IVoltageSource;
            //var x = module.GetType().GetInterface("IVoltageSensor");
        }

        //TODO: GET SINGLE MODULE BY TYPE
    }
}
