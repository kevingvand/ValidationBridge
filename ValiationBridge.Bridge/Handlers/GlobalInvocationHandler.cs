using System;
using System.Collections.Generic;
using System.Linq;
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

            switch (invokeMessage.MethodName)
            {
                case Constants.Commands.GetModules:
                    return GetModules();
                case Constants.Commands.GetModule:
                    return GetModule(invokeMessage.Arguments);
                case Constants.Commands.GetModuleInterfaces:
                    return GetModuleInterfaces(invokeMessage.Arguments);
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
            if (arguments.Length < 1)
            {
                _logService.LogError("Unable to retrieve module, no name specified");
                return null;
            }

            var argument = arguments[0];
            if(argument.Type != EType.STRING)
            {
                _logService.LogError($"Unable to retrieve module, name specified but in wrong format (got {argument.Type}), expected {EType.STRING}).");
                return null;
            }

            IModule moduleInstance = GetModuleInstanceByName(argument.GetValue<string>());

            //TODO: replace once real adapter is in place.
            //var loadedModules = _adapters.SelectMany(adapter => adapter.LoadedModules);
            //var module = loadedModules.FirstOrDefault(adapter => adapter.Name.Equals(nameArgument.GetValue<string>()));

            var instanceId = _instanceManager.CreateInstance(moduleInstance);
            return new ResultMessage(new Argument(instanceId));

            //TODO: check for 2nd argument (type) (if specified, else just return IModule?)
        }

        private IModule GetModuleInstanceByName(string name)
        {
            BaseAdapter moduleAdapter = _adapters.FirstOrDefault(adapter => adapter.LoadedModules.Any(loadedModule => loadedModule.GetName().Equals(name)));

            if (moduleAdapter == null)
            {
                _logService.LogError($@"Unable to retrieve module, module with name: ""{name}"" not found.");
                return null;
            }

            return moduleAdapter.GetModule(name);
        }

        private IModule GetModuleInstanceByInstanceId(Guid instanceId)
        {
            var instance = _instanceManager.GetInstance(instanceId);
            
            if(instance == null)
            {
                _logService.LogError($@"Instance with id: ""{instanceId}"" not found.");
                return null;
            }

            return instance.Instance as IModule;
        }

        private ResultMessage GetModuleInterfaces(Argument[] arguments)
        {
            if (arguments.Length < 1)
            {
                _logService.LogError("Unable to retrieve module instance, no identifier specified");
                return null;
            }

            var instanceIdArgument = arguments[0];
            if (instanceIdArgument.Type != EType.HANDLE)
            {
                _logService.LogError($"Unable to retrieve module instance, identifier specified but in wrong format (got {instanceIdArgument.Type}), expected {EType.HANDLE}).");
                return null;
            }

            var instance = _instanceManager.GetInstance(instanceIdArgument.GetValue<Guid>());
            if(instance == null)
            {
                _logService.LogError($"No instance found with the specified Id ({instanceIdArgument.GetValue<Guid>()})");
                return null;
            }

            var interfaces = instance.HandleType.GetInterfaces().Select(moduleInterface => moduleInterface.AssemblyQualifiedName);
            return new ResultMessage(new Argument(interfaces.ToArray()));
        }

    }
}
