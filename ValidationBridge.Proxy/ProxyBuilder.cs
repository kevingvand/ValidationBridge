using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ValidationBridge.Proxy
{
    public class ProxyBuilder<TInterface>
    {
        public const string DefaultAssemblyModuleName = "Main";

        private Dictionary<string, Action<object>> _definedMembers;
        private TypeBuilder _typeBuilder;
        private List<Type> _implementedInterfaces;
        private Type _compiledType;
        public bool Compiled { get; set; }

        public Guid InstanceId { get; set; }

        public ProxyBuilder(Guid instanceId, string typeSuffix = null)
        {
            InstanceId = instanceId;

            _definedMembers = new Dictionary<string, Action<object>>();
            _typeBuilder = GetTypeBuilder(typeSuffix);
            _implementedInterfaces = new List<Type>();

            AddProperty("InstanceId", typeof(Guid), true, false, InstanceId);
        }

        public TInterface GetInstance()
        {
            if (!Compiled)
                Compile();

            var moduleInstance = Activator.CreateInstance(_compiledType);

            foreach (var member in _definedMembers.Values)
                member?.Invoke(moduleInstance);

            return (TInterface)moduleInstance;
        }

        public Type GetCompiledType()
        {
            if (!Compiled)
                Compile();

            return _compiledType;
        }

        public Action<object> GetInitializer()
        {
            return (instance) =>
            {
                foreach (var member in _definedMembers.Values)
                    member?.Invoke(instance);
            };
        }

        public void CreateProxy(Type type, ProxyFunction methodBody)
        {
            if (Compiled)
                throw new Exception("Cannot add methods to the compiled proxy, please create a new instance.");

            if (_implementedInterfaces.Contains(type)) return;
            DefineProxyForType(type, methodBody);

            foreach (var parentInterface in type.GetInterfaces())
            {
                if (_implementedInterfaces.Contains(parentInterface)) continue;
                DefineProxyForType(parentInterface, methodBody);
            }
        }

        private void Compile()
        {
            if (!Compiled)
            {
                Compiled = true;

                _compiledType = Type.GetType(_typeBuilder.AssemblyQualifiedName);

                if (_compiledType == null)
                    _compiledType = _typeBuilder.CreateType();
            }
        }

        private void DefineProxyForType(Type type, ProxyFunction methodBody)
        {
            _implementedInterfaces.Add(type);
            _typeBuilder.AddInterfaceImplementation(type);

            var moduleInfo = type.GetMembers();

            var methods = moduleInfo.Where(info => info.MemberType == MemberTypes.Method);
            var properties = moduleInfo.Where(info => info.MemberType == MemberTypes.Property);

            foreach (var method in methods)
                CreateMethodProxy((MethodInfo)method, methodBody);

            foreach (var property in properties)
                CreatePropertyProxy((PropertyInfo)property);
        }

        private void CreateMethodProxy(MethodInfo method, ProxyFunction methodBody)
        {
            if (_definedMembers.ContainsKey(method.Name)) return; //TODO: error handling if 2 methods with same name

            var parameterTypes = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
            var parameterCount = parameterTypes.Length;

            var methodFieldName = GetFieldName(method.Name);
            var methodField = _typeBuilder.DefineField(methodFieldName, typeof(ProxyFunction), FieldAttributes.Private);

            MethodBuilder methodBuilder = _typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, parameterTypes);
            ILGenerator methodGenerator = methodBuilder.GetILGenerator();

            methodGenerator.Emit(OpCodes.Ldarg_0);
            methodGenerator.Emit(OpCodes.Ldfld, methodField);
            methodGenerator.Emit(OpCodes.Ldstr, InstanceId.ToString());
            methodGenerator.Emit(OpCodes.Ldstr, method.Name);

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

            if (method.ReturnType != typeof(void))
                methodGenerator.Emit(OpCodes.Unbox_Any, method.ReturnType);
            else
                methodGenerator.Emit(OpCodes.Pop);

            methodGenerator.Emit(OpCodes.Ret);

            _definedMembers.Add(method.Name, GetInitializeFieldAction(methodFieldName, methodBody));
        }

        private void CreatePropertyProxy(PropertyInfo property)
        {
            PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, Type.EmptyTypes);
        }

        public void AddProperty(string name, Type propertyType, bool allowGet = true, bool allowSet = false, object defaultValue = null)
        {
            var propertyFieldName = GetFieldName(name);
            FieldBuilder propertyFieldBuilder = _typeBuilder.DefineField(propertyFieldName, propertyType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, propertyType, Type.EmptyTypes);

            if (allowGet)
            {
                MethodBuilder propertyGetMethodBuilder = _typeBuilder.DefineMethod($"get_{name}", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
                ILGenerator propertyGetMethodGenerator = propertyGetMethodBuilder.GetILGenerator();

                propertyGetMethodGenerator.Emit(OpCodes.Ldarg_0);
                propertyGetMethodGenerator.Emit(OpCodes.Ldfld, propertyFieldBuilder);
                propertyGetMethodGenerator.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(propertyGetMethodBuilder);
            }


            if (allowSet)
            {
                MethodBuilder propertySetMethodBuilder = _typeBuilder.DefineMethod($"set_{name}", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, typeof(void), new Type[] { propertyType });
                ILGenerator propertySetMethodGenerator = propertySetMethodBuilder.GetILGenerator();

                propertySetMethodGenerator.Emit(OpCodes.Ldarg_0);
                propertySetMethodGenerator.Emit(OpCodes.Ldarg_1);
                propertySetMethodGenerator.Emit(OpCodes.Stfld, propertyFieldBuilder);
                propertySetMethodGenerator.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(propertySetMethodBuilder);
            }

            if (defaultValue != null)
                _definedMembers.Add(name, GetInitializeFieldAction(propertyFieldName, defaultValue));
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

        private TypeBuilder GetTypeBuilder(string typeSuffix = null)
        {
            var typeSignature = $"{GetType().Namespace}.{typeof(TInterface).Name}{(typeSuffix != null ? $"_{typeSuffix}" : string.Empty)}";
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(typeSignature), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(DefaultAssemblyModuleName);
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeSignature, TypeAttributes.Public | TypeAttributes.Class);

            return typeBuilder;
        }
    }
}
