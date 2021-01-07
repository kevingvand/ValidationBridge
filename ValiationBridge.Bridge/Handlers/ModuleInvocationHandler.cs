﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Services;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Messages;

namespace ValiationBridge.Bridge.Handlers
{
    public class ModuleInvocationHandler : MessageHandler
    {
        private LogService _logService;
        private InstanceManager _instanceManager;

        public ModuleInvocationHandler(InstanceManager instanceManager)
        {
            _logService = new LogService();
            _instanceManager = instanceManager;
        }

        public override PipeMessage HandleMessages(PipeMessage message)
        {
            if (!(message is InvokeMessage invokeMessage)) return null;
            if (invokeMessage.InstanceId == Guid.Empty) return null;

            var instance = _instanceManager.GetInstance(invokeMessage.InstanceId);

            var method = instance.HandleType.GetMethod(invokeMessage.MethodName);
            if(method == null)
            {
                _logService.LogError("Could not invoke method on module, method not found!");
                return null;
            }

            var parameters = invokeMessage.Arguments.Select(argument => argument.Value).ToArray();

            var result = method.Invoke(instance.Instance, parameters);
            return new ResultMessage(new Argument(ETypeExtension.FromSystemType(method.ReturnType), result));
        }
    }
}