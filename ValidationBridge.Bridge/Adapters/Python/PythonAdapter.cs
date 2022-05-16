using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ValidationBridge.Bridge.Services;
using ValidationBridge.Common.Interfaces.Modules;
using ValidationBridge.Common.Services;
using ValidationBridge.Proxy;

namespace ValidationBridge.Bridge.Adapters.Python
{
    //TODO: a lot of duplicate code in comparison to MatlabAdapter, consider generalization?
    public class PythonAdapter : BaseAdapter
    {
        private LogService _logService;

        private Dictionary<Type, Action<object>> _initializerMap;

        public PythonAdapter()
        {
            _logService = new LogService();

            _initializerMap = new Dictionary<Type, Action<object>>();

        }

        public override IModule GetModule(string name)
        {
            var moduleType = LoadedModules[name];
            var module = base.GetModule(name);

            if (module == null) return null;

            _initializerMap[moduleType](module);

            return module;
        }

        public override List<string> LoadModule(string modulePath)
        {
            var console = new PythonConsole("LOAD_PYTHON_MODULES");

            if (!console.IsInstalled())
            {
                _logService.LogWarning($"Python is not installed or configured incorrectly, no Python modules can be loaded.");
                console.Stop();
                return new List<string>();
            }

            var moduleDirectoryInfo = new DirectoryInfo(modulePath);
            if (!moduleDirectoryInfo.Exists)
            {
                console.Stop();
                return new List<string>();
            }

            var validModules = GetValidModules(modulePath, console);

            foreach (var moduleInfo in validModules)
            {
                var moduleProxyType = CreateModuleProxy(moduleInfo);
                if (LoadedModules.ContainsKey(moduleInfo.ModuleName))
                {
                    _logService.LogWarning($@"Python module with name: ""{moduleInfo.ModuleName}"" already exists, replacing.");
                    LoadedModules[moduleInfo.ModuleName] = moduleProxyType;
                }
                else LoadedModules.Add(moduleInfo.ModuleName, moduleProxyType);
            }

            console.Stop();
            return validModules.Select(x => x.ModuleName).ToList();
        }

        private Type CreateModuleProxy(ProxyModuleInformation moduleInfo)
        {
            var proxy = new ProxyBuilder<IModule>(Guid.NewGuid(), $"{typeof(BaseAdapter).Namespace}.Proxy", moduleInfo.ModuleName);

            foreach (var moduleInterface in moduleInfo.Interfaces)
            {
                proxy.CreateProxy(moduleInterface, new ProxyFunction((instanceId, methodName, arguments) =>
                {
                    // Start a python interactive console or attach to an existing
                    var console = PythonInstanceManager.GetInstance(instanceId);

                    // Parse information of executing instance
                    var instanceIdGuid = Guid.Parse(instanceId);
                    var instanceVariable = $"m_{instanceIdGuid.ToString("N")}";

                    // Check if the instance was already created in matlab, if not create it
                    var instanceExists = console.Evaluate($"'{instanceVariable}' in globals()");
                    if (!instanceExists)
                    {
                        // Add the path of the module to the system paths
                        DirectoryInfo moduleDirectoryInfo = new DirectoryInfo(moduleInfo.PackagePath);
                        console.AddPath(moduleDirectoryInfo.Parent.FullName);

                        // Import the module and create a new instance
                        var moduleImport = moduleInfo.ClassName.Substring(0, moduleInfo.ClassName.LastIndexOf('.'));
                        console.Import(moduleImport);
                        console.DefineVariable(instanceVariable, moduleInfo.ClassName + "()");
                    }

                    // Parse the arguments to a format understandable for Python
                    List<string> stringArguments = new List<string>();
                    foreach (var argument in arguments)
                    {
                        // If the argument is an array, an array representation has to be created
                        if (argument.GetType().IsArray)
                        {
                            var argumentArray = ((Array) argument).Cast<object>();

                            // If the value is a string, it should be surrounded by quotes, otherwise it has to be converted using the invariant culture
                            var arrayDefinition = argument.GetType().GetElementType() == typeof(string)
                                ? string.Join(",", 
                                    argumentArray.Select(element => $"\"{element}\""))
                                : string.Join(",",
                                    argumentArray.Select(element => Convert.ToString(element, CultureInfo.InvariantCulture)));
                            stringArguments.Add($"[{arrayDefinition}]");
                        }
                        // If the argument is a string, it has to be surrounded by quotes
                        else if (argument.GetType() == typeof(string)) stringArguments.Add($"\"{argument}\"");
                        // If the argument is not an array or a string, it has to be converted using the invariant culture
                        else stringArguments.Add(Convert.ToString(argument, CultureInfo.InvariantCulture));
                    }

                    // Build the invocation command and evaluate.
                    var argumentsString = string.Join(", ", stringArguments);
                    var result = console.Evaluate($"{instanceVariable}.{methodName}({argumentsString})");

                    // Get the return type of function from the interface, and return null if the return type is void
                    var returnType = ModuleService.GetPublicMethod(moduleInterface, methodName).ReturnType;
                    if (returnType.Equals(typeof(void))) return null;

                    // Convert the result to the expected type and return.
                    return Convert.ChangeType(result, returnType);
                }));
            }

            var moduleType = proxy.GetCompiledType();
            _initializerMap.Add(moduleType, proxy.GetInitializer());
            return moduleType;
        }

        private List<ProxyModuleInformation> GetValidModules(string modulePath, PythonConsole console)
        {
            var modules = new List<ProxyModuleInformation>();

            // Retrieve the module package for the specified path and retrieve the package name
            DirectoryInfo moduleDirectoryInfo = new DirectoryInfo(modulePath);
            var packageName = moduleDirectoryInfo.Name;

            // Retrieve all python files within the package and convert them to namespaces
            var moduleImports = moduleDirectoryInfo.GetFiles("*.py").Select(moduleFile => $"{packageName}.{Path.GetFileNameWithoutExtension(moduleFile.Name)}");

            // If there are no python files, we do not have to check for modules
            if (!moduleImports?.Any() ?? false) return modules;

            // Start the interactive console
            console.Start();

            // Load the parent folder of the specified module
            console.AddPath(moduleDirectoryInfo.Parent.FullName);

            // Import inspect for reflection functionality, and all previously loaded module candidates
            console.Import("inspect");
            console.Import(moduleImports.ToArray());

            // Get all loaded modules from the package
            console.DefineVariable("modules", $"inspect.getmembers({packageName}, inspect.ismodule)");

            // Get all classes within the modules
            console.DefineVariable("moduleClasses", $"[moduleClass for moduleName, module in modules for moduleClassName, moduleClass in inspect.getmembers(module, inspect.isclass)]");

            // Filter all classes without the "implements" property
            console.DefineVariable("moduleClasses", "list(filter(lambda moduleClass: hasattr(moduleClass, 'implements'), moduleClasses))");

            // Get number of candidate modules
            var moduleCandidateCount = console.Evaluate("moduleClasses.__len__()");


            // Loop throug all modules
            for (int moduleIndex = 0; moduleIndex < moduleCandidateCount; moduleIndex++)
            {
                // Retrieve the module class name and save in a proxy module information container
                console.DefineVariable($"moduleClass", $"moduleClasses[{moduleIndex}]");
                var moduleClassName = console.Evaluate($"moduleClass.__module__ + '.' + moduleClass.__name__");
                var moduleInfo = new ProxyModuleInformation(moduleClassName, modulePath);

                var x = moduleClassName.ToCharArray();
                console.DefineVariable("moduleInstance", $"{moduleClassName}()");

                // Get number of implemented interfaces
                var interfaceCount = console.Evaluate("moduleClass.implements.__len__()");

                // Assume module is valid, until proven otherwise
                var validModule = true;

                // Loop throug all interfaces
                for (int interfaceIndex = 0; interfaceIndex < interfaceCount; interfaceIndex++)
                {
                    // Get interface name
                    var interfaceName = console.Evaluate($"moduleClass.implements[{interfaceIndex}]");

                    // Get interface type
                    Type interfaceType = ModuleService.GetModuleType(interfaceName); //TODO: change if interfaces can be loaded dynamically.

                    // Check if the type was found, else module definition is invalid.
                    if (interfaceType == null)
                    {
                        _logService.LogWarning($@"Attempting to load module: ""{moduleInfo.ClassName}"", unknown interface with name ""{interfaceName}"" encountered.");
                        continue;
                    }

                    // add to list of interfaces for module
                    moduleInfo.Interfaces.Add(interfaceType);

                    // Get members to validate
                    var properties = ModuleService.GetPublicProperties(interfaceType);
                    var methods = ModuleService.GetPublicMethods(interfaceType);

                    // Assume properties and methods are valid, until proven otherwise.
                    var validProperties = true;
                    var validMethods = true;

                    // Loop through all properties
                    foreach (var property in properties)
                    {
                        // Ignore the InstanceId property
                        if (property.Name.Equals(nameof(IModule.InstanceId))) continue;

                        if (!console.Evaluate($"hasattr(moduleInstance, '{property.Name}')") || console.Evaluate($"callable(moduleInstance.{property.Name})"))
                        {
                            validProperties = false;
                            break;
                        }
                    }

                    // Do not validate methods if there is an invalid property.
                    if (!validProperties)
                    {
                        validModule = false;
                        break;
                    }

                    // Loop through all methods
                    foreach (var method in methods)
                    {
                        // Ignore getter and setter methods
                        if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))) continue;

                        if (!console.Evaluate($"hasattr(moduleInstance, '{method.Name}')") || !console.Evaluate($"callable(moduleInstance.{method.Name})"))
                        {
                            validMethods = false;
                            break;
                        }


                        // check if the number of arguments match the number of arguments of the current method, if not the module is invalid.
                        var argumentCount = console.Evaluate($"inspect.getargspec(moduleInstance.{method.Name}).args.__len__()");
                        if (argumentCount != method.GetParameters().Count() + 1)
                        {
                            validMethods = false;
                            break;
                        }
                    }

                    // stop validating module if there is an invalid method.
                    if (!validMethods)
                    {
                        validModule = false;
                        break;
                    }
                }

                // If the module is valid, retrieve its real name and add it to the result list
                if (validModule)
                {
                    moduleInfo.ModuleName = console.Evaluate("moduleInstance.GetName()");
                    modules.Add(moduleInfo);
                }
            }

            // Stop the interactive console and return the result list
            console.Stop();
            return modules;
        }
    }
}
