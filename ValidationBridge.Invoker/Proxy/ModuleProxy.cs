using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using ValidationBridge.Common;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Interfaces.Modules;
using ValidationBridge.Common.Messages;

namespace ValidationBridge.Invoker.Proxy
{
    public class ModuleProxy<TModule>
    {
        private Guid _instanceId;
        private TypeBuilder _typeBuilder;
        private Dictionary<string, Action<object>> _definedMembers;
        private Type _compiledType;

        public bool Compiled { get; set; }

        public ModuleProxy(Guid instanceId)
        {
            _definedMembers = new Dictionary<string, Action<object>>();
            _instanceId = instanceId;
            _typeBuilder = GetTypeBuilder();
        }

        public TModule GetModule()
        {
            if (!Compiled)
            {
                Compiled = true;

                _compiledType = Type.GetType(_typeBuilder.AssemblyQualifiedName);

                if (_compiledType == null)
                    _compiledType = _typeBuilder.CreateType();
            }

            var moduleInstance = Activator.CreateInstance(_compiledType);

            foreach (var member in _definedMembers.Values)
                member?.Invoke(moduleInstance);

            return (TModule)moduleInstance;
        }

        public void CreateProxy(BridgeClient target)
        {
            if (Compiled)
                throw new Exception("Cannot add methods to the compiled proxy, please create a new instance.");

            _typeBuilder.AddInterfaceImplementation(typeof(TModule));

            var moduleInfo = GetMemberInfo(typeof(TModule));

            var methods = moduleInfo.Where(info => info.MemberType == MemberTypes.Method);
            var properties = moduleInfo.Where(info => info.MemberType == MemberTypes.Property);

            foreach(var method in methods)
                CreateMethodProxy((MethodInfo)method, target, _instanceId);

            foreach(var property in properties)
                    CreatePropertyProxy((PropertyInfo)property, target, _instanceId);
        }

        private void CreateMethodProxy(MethodInfo info, BridgeClient target, Guid instanceId)
        {
            if (_definedMembers.ContainsKey(info.Name)) return; //TODO: error handling if 2 methods with same name

            var parameterTypes = info.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
            var parameterCount = parameterTypes.Length;

            var proxyBody = new ProxyFunction((proxyParameters) =>
            {
                var arguments = proxyParameters.Select(parameter => new Argument(ETypeExtension.FromSystemType(parameter.GetType()), parameter)).ToArray();
                var invokeMessage = new InvokeMessage(instanceId, info.Name, arguments);
                var result = target.WriteMessage(invokeMessage);

                if (result == null || result.MessageType != EMessageType.RESULT)
                {
                    //TODO: error handling
                    throw new Exception("Invalid return message received from target.");
                }

                return result.Result.Value;
            });

            var proxyInputs = new ParameterExpression[parameterCount];

            for (int i = 0; i < parameterCount; i++)
                proxyInputs[i] = Expression.Parameter(typeof(object));

            var proxyCallExpression = Expression.Call(Expression.Constant(proxyBody.Target), proxyBody.Method, Expression.NewArrayInit(typeof(object), proxyInputs));
            var proxyCallDelegate = Expression.Lambda(proxyCallExpression, proxyInputs).Compile();

            var methodFieldName = GetFieldName(info.Name);
            var methodField = _typeBuilder.DefineField(methodFieldName, typeof(ProxyFunction), FieldAttributes.Private);

            MethodBuilder methodBuilder = _typeBuilder.DefineMethod(info.Name, MethodAttributes.Public | MethodAttributes.Virtual, info.ReturnType, parameterTypes);
            ILGenerator methodGenerator = methodBuilder.GetILGenerator();

            methodGenerator.Emit(OpCodes.Ldarg_0);
            methodGenerator.Emit(OpCodes.Ldfld, methodField);

            // Array Creation
            methodGenerator.Emit(OpCodes.Ldc_I4, parameterCount);
            methodGenerator.Emit(OpCodes.Newarr, typeof(object));

            for (int i = 1; i <= parameterCount; i++)
            {
                methodGenerator.Emit(OpCodes.Dup);
                methodGenerator.Emit(OpCodes.Ldc_I4, i - 1);
                methodGenerator.Emit(OpCodes.Ldarg, i);
                methodGenerator.Emit(OpCodes.Box, parameterTypes[i - 1]);
                methodGenerator.Emit(OpCodes.Stelem_Ref);
            }

            methodGenerator.Emit(OpCodes.Call, typeof(ProxyFunction).GetMethod("Invoke"));

            if (info.ReturnType != typeof(void))
                methodGenerator.Emit(OpCodes.Unbox_Any, info.ReturnType);
            else
                methodGenerator.Emit(OpCodes.Pop);

            methodGenerator.Emit(OpCodes.Ret);

            _definedMembers.Add(info.Name, GetInitializeFieldAction(methodFieldName, proxyBody));
        }

        private void CreatePropertyProxy(PropertyInfo info, BridgeClient target, Guid instanceId)
        {
            PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(info.Name, PropertyAttributes.HasDefault, info.PropertyType, Type.EmptyTypes);
        }

        private List<MemberInfo> GetMemberInfo(Type type)
        {
            var members = type.GetMembers().ToList();
            var interfaces = type.GetInterfaces();

            foreach (var moduleInterface in interfaces)
                members.AddRange(moduleInterface.GetMembers());

            return members;
        }

        private TypeBuilder GetTypeBuilder()
        {
            var typeSignature = $"{GetType().Namespace}.{typeof(TModule).Name}_{_instanceId.ToString("N")}";
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(typeSignature), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(Constants.DefaultAssemblyModuleName);
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeSignature, TypeAttributes.Public | TypeAttributes.Class);

            string instanceIdName = nameof(IModule.InstanceId);
            string instanceIdFieldName = GetFieldName(instanceIdName);
            FieldBuilder instanceIdFieldBuilder = typeBuilder.DefineField(instanceIdFieldName, typeof(Guid), FieldAttributes.Private);
            PropertyBuilder instanceIdPropertyBuilder = typeBuilder.DefineProperty(instanceIdName, PropertyAttributes.HasDefault, typeof(Guid), null);
            MethodBuilder instanceIdGetMethodBuilder = typeBuilder.DefineMethod($"get_{instanceIdName}", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, typeof(Guid), Type.EmptyTypes);
            ILGenerator instanceIdGetGenerator = instanceIdGetMethodBuilder.GetILGenerator();

            instanceIdGetGenerator.Emit(OpCodes.Ldarg_0);
            instanceIdGetGenerator.Emit(OpCodes.Ldfld, instanceIdFieldBuilder);
            instanceIdGetGenerator.Emit(OpCodes.Ret);

            instanceIdPropertyBuilder.SetGetMethod(instanceIdGetMethodBuilder);

            _definedMembers.Add(instanceIdName, GetInitializeFieldAction(instanceIdFieldName, _instanceId));

            return typeBuilder;
        }

        private Action<object> GetInitializeFieldAction(string fieldName, object value)
        {
            return proxyInstance =>
            {
                proxyInstance
                    .GetType()
                    .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(proxyInstance, value);
            };
        }

        private string GetFieldName(string name)
        {
            return $"_{char.ToLowerInvariant(name[0])}{name.Substring(1)}";
        }
    }
}
