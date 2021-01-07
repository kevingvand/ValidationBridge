using System;
using System.Collections.Generic;
using System.IO;
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
                case Constants.Commands.LoadModule:
                    return LoadModule(invokeMessage.Arguments);
                case Constants.Commands.LoadModules:
                    return LoadModules(invokeMessage.Arguments);
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

        private PipeMessage LoadModules(Argument[] arguments)
        {
            if (arguments.Length < 1)
                return GetErrorMessage("Unable to load modules, no path specified");

            var argument = arguments[0];
            if (argument.Type != EType.STRING)
                return GetErrorMessage($"Unable to load modules, path specified but in wrong format (got {argument.Type}), expected {EType.STRING}).");

            throw new NotImplementedException();
            //TODO: finish implementing (load modules for all files in folder)
        }

        private PipeMessage LoadModule(Argument[] arguments)
        {
            if (arguments.Length < 1)
                return GetErrorMessage("Unable to load module, no path specified");

            var argument = arguments[0];
            if (argument.Type != EType.STRING)
                return GetErrorMessage($"Unable to load module, path specified but in wrong format (got {argument.Type}), expected {EType.STRING}).");

            string modulePath = argument.GetValue<string>();
            if (!File.Exists(modulePath))
                return GetErrorMessage($"Unable to load module, specified file does not exist ({modulePath}).");

            List<string> loadedModules = new List<string>();
            foreach (var adapter in _adapters)
            {
                var loadedModule = adapter.LoadModule(modulePath);
                if (loadedModule != null)
                    loadedModules.AddRange(loadedModule);
            }

            if (loadedModules.Count == 0)
                _logService.LogWarning("Specified path did not contain any valid modules to load.");
            else
                _logService.LogInfo($"Successfully loaded {loadedModules.Count} module(s).");
            return new ResultMessage(new Argument(loadedModules.ToArray()));
        }

        private ResultMessage GetModules()
        {
            var loadedModules = _adapters
                .SelectMany(adapter => adapter.LoadedModules)
                .Select(adapter => adapter.GetName()).ToArray();

            return new ResultMessage(new Argument(loadedModules));
        }

        //TODO: split into 2: Get module (retrieves an already created module) and Create Module (creates a new instance of a module)
        private PipeMessage GetModule(Argument[] arguments)
        {
            if (arguments.Length < 1)
                return GetErrorMessage("Unable to retrieve module, no name specified");

            if (arguments.Length < 2)
                return GetErrorMessage("Unable to retrieve module, no type specified");

            var nameArgument = arguments[0];
            if (nameArgument.Type != EType.STRING)
                return GetErrorMessage($"Unable to retrieve module, name specified but in wrong format (got {nameArgument.Type}), expected {EType.STRING}).");

            var typeArgument = arguments[1];
            if (nameArgument.Type != EType.STRING)
                return GetErrorMessage($"Unable to retrieve module, type specified but in wrong format (got {typeArgument.Type}), expected {EType.STRING}).");

            var type = Type.GetType(typeArgument.GetValue<string>());
            if (type == null)
                return GetErrorMessage("Unable to retrieve module, type specified does not exist.");


            var moduleName = nameArgument.GetValue<string>();


            return GetModuleInstanceByName(moduleName, type);

            //TODO: check for 2nd argument (type) (if specified, else just return IModule?)
        }

        private PipeMessage GetModuleInstanceByName(string name, Type type)
        {
            BaseAdapter moduleAdapter = _adapters.FirstOrDefault(adapter => adapter.LoadedModules.Any(loadedModule => loadedModule.GetName().Equals(name)));

            if (moduleAdapter == null)
                return GetErrorMessage($@"Unable to retrieve module, module with name: ""{name}"" not found.");

            IModule moduleInstance = moduleAdapter.GetModule(name);

            if (moduleInstance == null)
                return GetErrorMessage($@"Unable to retrieve module, module with name ""{name}"" exists, but was not able to load.");

            var implementedInterface = moduleInstance.GetType().GetInterface(type.Name);
            if (implementedInterface == null)
                return GetErrorMessage($@"Unable to retrieve module, the specified module cannot be used as ""{type.Name}""");

            var instanceId = _instanceManager.CreateInstance(moduleInstance);
            return new ResultMessage(new Argument(instanceId));
        }

        private PipeMessage GetModuleInterfaces(Argument[] arguments)
        {
            if (arguments.Length < 1)
                return GetErrorMessage("Unable to retrieve module instance, no identifier specified");

            var instanceIdArgument = arguments[0];
            if (instanceIdArgument.Type != EType.HANDLE)
                return GetErrorMessage($"Unable to retrieve module instance, identifier specified but in wrong format (got {instanceIdArgument.Type}), expected {EType.HANDLE}).");

            var instance = _instanceManager.GetInstance(instanceIdArgument.GetValue<Guid>());
            if (instance == null)
                return GetErrorMessage($"No instance found with the specified Id ({instanceIdArgument.GetValue<Guid>()})");

            var interfaces = instance.HandleType.GetInterfaces().Select(moduleInterface => moduleInterface.AssemblyQualifiedName);
            return new ResultMessage(new Argument(interfaces.ToArray()));
        }

        private ErrorMessage GetErrorMessage(string message)
        {
            var errorMessage = new ErrorMessage(message);
            return errorMessage;
        }
    }
}
