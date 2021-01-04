using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ImpromptuInterface;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Messages;

namespace ValidationBridge.Invoker.Proxy
{
    public class ModuleProxy
    {
        private ICollection<KeyValuePair<string, object>> _members;
        public ExpandoObject Instance { get; set; }

        public ModuleProxy()
        {
            Instance = new ExpandoObject();
            _members = Instance;
        }

        public void AddMember(string name, object value)
        {
            _members.Add(new KeyValuePair<string, object>(name, value));
        }

        public TModule GetModule<TModule>()
        {
            return Instance.ActLike(typeof(TModule));
        }

        public dynamic GetModule(Type moduleType)
        {
            return Instance.ActLike(moduleType);
        }

        public void CreateProxy(Type proxyType, BridgeClient target, Guid instanceId)
        {
            var moduleInfo = GetMemberInfo(proxyType);

            foreach(var moduleMember in moduleInfo)
            {
                if (moduleMember.MemberType == MemberTypes.Method)
                    CreateMethodProxy((MethodInfo)moduleMember, target, instanceId);

                if (moduleMember.MemberType == MemberTypes.Property)
                {
                    throw new Exception("Modules currently cannot have properties.");
                    //TODO: allow properties?
                    //CreatePropertyProxy((PropertyInfo)moduleMember, target, instanceId);
                }
            }
        }

        private void CreateMethodProxy(MethodInfo info, BridgeClient target, Guid instanceId)
        {
            var parameterCount = info.GetParameters().Count();

            var proxyBody = new ProxyFunction((proxyParameters) =>
            {
                var arguments = proxyParameters.Select(parameter => new Argument(ETypeExtension.FromSystemType(parameter.GetType()), parameter)).ToArray();
                var invokeMessage = new InvokeMessage(instanceId, info.Name, arguments);
                var result = target.WriteMessage(invokeMessage);

                if(result == null || result.MessageType != EMessageType.RESULT)
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
            AddMember(info.Name, Expression.Lambda(proxyCallExpression, proxyInputs).Compile());
        }

        private void CreatePropertyProxy(PropertyInfo info, BridgeClient target, Guid instanceId)
        {
            //TODO: implement...
        }

        private List<MemberInfo> GetMemberInfo(Type type)
        {
            var members = type.GetMembers().ToList();
            var interfaces = type.GetInterfaces();

            foreach (var moduleInterface in interfaces)
                members.AddRange(moduleInterface.GetMembers());

            return members;
        }
    }
}
