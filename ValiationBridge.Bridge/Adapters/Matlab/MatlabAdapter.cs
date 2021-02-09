using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ValiationBridge.Bridge.Services;
using ValidationBridge.Common.Interfaces.Modules;
using ValidationBridge.Common.Services;
using ValidationBridge.Proxy;

namespace ValiationBridge.Bridge.Adapters.Matlab
{
    public class MatlabAdapter : BaseAdapter
    {
        private LogService _logService;

        private Dictionary<Type, Action<object>> _initializerMap;

        public MatlabAdapter()
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
            if (!Matlab.IsInstalled())
            {
                _logService.LogWarning($"MATLAB is not installed, no MATLAB modules can be loaded.");
                return new List<string>();
            }

            var directoryInfo = new DirectoryInfo(modulePath);

            if (!directoryInfo.Exists || !directoryInfo.Name.StartsWith("+"))
                return new List<string>();

            var packagePath = directoryInfo.Parent.FullName;
            var packageName = directoryInfo.Name.Substring(1);

            var validModules = GetValidModules(packagePath, packageName);

            foreach (var moduleInfo in validModules)
            {
                var moduleProxyType = CreateModuleProxy(moduleInfo);
                if (LoadedModules.ContainsKey(moduleInfo.ModuleName))
                {
                    _logService.LogWarning($@"Matlab module with name: ""{moduleInfo.ModuleName}"" already exists, replacing.");
                    LoadedModules[moduleInfo.ModuleName] = moduleProxyType;
                }
                else LoadedModules.Add(moduleInfo.ModuleName, moduleProxyType);
            }

            return validModules.Select(x => x.ModuleName).ToList();
        }

        private Type CreateModuleProxy(ProxyModuleInformation moduleInfo)
        {
            var proxy = new ProxyBuilder<IModule>(Guid.NewGuid(), $"{typeof(BaseAdapter).Namespace}.Proxy", moduleInfo.ModuleName);

            foreach (var moduleInterface in moduleInfo.Interfaces)
            {
                //TODO: handle properties?
                proxy.CreateProxy(moduleInterface, new ProxyFunction((instanceId, methodName, arguments) =>
                {
                    //TODO: add path?

                    // Start matlab or attach to running instance
                    var matlab = new Matlab();

                    // Parse information of executing instance
                    var instanceIdGuid = Guid.Parse(instanceId);
                    var instanceVariable = $"m_{instanceIdGuid.ToString("N")}";

                    // Check if the instance was already created in matlab, if not create it
                    matlab.Execute("if(exist('instances', 'var') == 0); instances = struct(); end;");
                    matlab.Execute($"if(~isfield(instances, '{instanceVariable}')); instances.{instanceVariable} = {moduleInfo.ClassName}; end;");

                    // Retrieve meta information of instance
                    matlab.Execute($"moduleMetaInfo = metaclass(instances.{instanceVariable});");
                    matlab.Execute($@"moduleMetaMethodInfo = findobj(moduleMetaInfo.MethodList, ""Name"", ""{methodName}"")");

                    var argumentString = string.Join(", ", arguments.Select(argument => GetArgumentAsString(argument)));

                    int outputLength = (int)matlab.EvaluateExpression("outputLength", "size(moduleMetaMethodInfo.OutputNames, 1)");

                    if (outputLength > 0)
                    {
                        string outputName = matlab.EvaluateExpression("outputName", "moduleMetaMethodInfo.OutputNames{1}", true);
                        matlab.Execute("inputNames = string(moduleMetaMethodInfo.InputNames)");
                        string[] inputNames = matlab.GetVariable<string[]>("inputNames");
                        matlab.Execute("clear inputNames");

                        if (outputName.Equals(inputNames[0]))
                        {
                            matlab.Execute($@"instances.{instanceVariable} = instances.{instanceVariable}.{methodName}{(arguments.Length > 0 ? $"({argumentString})" : string.Empty)}");
                            return null;
                        }

                        //TODO: allow for multiple result values?
                        matlab.Execute($@"methodInvocationResult = instances.{instanceVariable}.{methodName}{(arguments.Length > 0 ? $"({argumentString})" : string.Empty)}");
                        var returnType = ModuleService.GetPublicMethod(moduleInterface, methodName).ReturnType;
                        return matlab.GetVariable("methodInvocationResult", returnType);
                    }

                    matlab.Execute($@"instances.{instanceVariable}.{methodName}{(arguments.Length > 0 ? $"({argumentString})" : string.Empty)}");
                    return null;
                }));
            }

            var moduleType = proxy.GetCompiledType();
            _initializerMap.Add(moduleType, proxy.GetInitializer());
            return moduleType;
        }

        private string GetArgumentAsString(object argument)
        {
            if (argument is string) return $@"""{argument}""";
            //TODO: if argument is array, add array definition;
            return Convert.ToString(argument, CultureInfo.InvariantCulture);
        }

        private List<ProxyModuleInformation> GetValidModules(string path, string package)
        {
            // Start matlab, or attach to running instance
            var matlab = new Matlab();
            matlab.SetVisible(true); //TODO: remove after debugging.

            // Add path to module
            matlab.AddPath(path);

            // Setup reflection workspace
            matlab.Execute(@"reflection = {};");

            // Retrieve package definition
            matlab.Execute($@"reflection.package = meta.package.fromName(""{package}"");");

            // Initialize reflection variables
            matlab.Execute("reflection.moduleCandidates = {}; reflection.moduleDefinitions = string.empty;");

            // Fill candidates list
            matlab.Execute("for i = 1:length(reflection.package.ClassList); c = reflection.package.ClassList(i); if any(strcmp({c.PropertyList.Name}, 'Implements')); reflection.moduleCandidates(end+1) = {c}; end; end;");

            // Get module count
            var moduleCandidateCount = matlab.EvaluateExpression("moduleCandidateCount", "size(reflection.moduleCandidates, 2)", true);

            List<ProxyModuleInformation> modules = new List<ProxyModuleInformation>();

            // Loop through all module definitions
            for (int moduleIndex = 0; moduleIndex < moduleCandidateCount; moduleIndex++)
            {
                // Retrieve module class name and save in a proxy module information container
                matlab.Execute($"moduleClass = reflection.moduleCandidates{{{moduleIndex + 1}}};");
                var moduleInfo = new ProxyModuleInformation(matlab.EvaluateExpression("moduleName", "moduleClass.Name", true));

                // Create instance of module
                matlab.Execute($@"moduleInstance = feval(""{moduleInfo.ClassName}"");");

                // Get number of implemented interfaces
                var interfaceCount = matlab.EvaluateExpression("moduleInterfaceCount", "size(moduleInstance.Implements, 2)", true);

                // Assume module is valid, until proven otherwise.
                var validModule = true;

                // Loop through all interfaces
                for (int interfaceIndex = 0; interfaceIndex < interfaceCount; interfaceIndex++)
                {
                    // Get interface name
                    var interfaceName = matlab.EvaluateExpression("interfaceName", $"moduleInstance.Implements({interfaceIndex + 1})", true);

                    // Get interface type and add to list of interfaces for module
                    Type interfaceType = ModuleService.GetModuleType(interfaceName); //TODO: change if interfaces can be loaded dynamically.
                    moduleInfo.Interfaces.Add(interfaceType);

                    // Check if the type was found, else module definition is invalid.
                    if (interfaceType == null)
                    {
                        _logService.LogWarning($@"Attempting to load module: ""{moduleInfo.ClassName}"", unknown interface with name ""{interfaceName}"" encountered.");
                        continue;
                    }

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

                        // Check if the module has the current property, if not the module is invalid.
                        if (!matlab.EvaluateExpression("hasMember", $@"any(strcmp({{moduleClass.PropertyList.Name}}, '{property.Name}'))", true))
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

                        // Save the method information
                        matlab.Execute($@"member = findobj(moduleClass.MethodList, ""Name"", ""{method.Name}"")");

                        // Check if the module has the current method, if not the module is invalid.
                        if (!matlab.EvaluateExpression("hasMember", "size(member, 1) ~= 0", true))
                        {
                            validMethods = false;
                            break;
                        }

                        // check if the number of arguments match the number of arguments of the current method, if not the module is invalid.
                        var argumentCount = matlab.EvaluateExpression("argumentCount", "size(member.InputNames, 1)", true);
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
                    moduleInfo.ModuleName = matlab.EvaluateExpression("moduleName", "moduleInstance.GetName", true);
                    modules.Add(moduleInfo);
                }
            }

            return modules;
        }

    }
}
