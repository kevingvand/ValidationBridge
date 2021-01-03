using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Adapters;
using ValidationBridge.Common;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Messages;

namespace ValiationBridge.Bridge.Handlers
{
    public class GlobalInvocationHandler : MessageHandler
    {
        private List<BaseAdapter> _adapters;

        public GlobalInvocationHandler(List<BaseAdapter> adapters)
        {
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
                default:
                    return null;
            }
        }

        private ResultMessage GetModules()
        {
            var loadedModules = _adapters
                .SelectMany(adapter => adapter.LoadedModules)
                .Select(adapter => adapter.Name).ToArray();

            return new ResultMessage(new Argument(loadedModules));
        }
    }
}
