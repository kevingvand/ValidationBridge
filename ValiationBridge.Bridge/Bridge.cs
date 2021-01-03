using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Adapters;
using ValiationBridge.Bridge.Handlers;
using ValiationBridge.Bridge.Services;

namespace ValiationBridge.Bridge
{
    public class Bridge
    {
        private LogService _logService { get; set; }


        public bool IsTerminated { get; set; }

        public BridgeServer Server { get; set; }

        public List<BaseAdapter> ModuleAdapters { get; set; }
        public List<MessageHandler> MessageHandlers { get; set; }

        public Bridge()
        {
            Server = new BridgeServer();
            ModuleAdapters = new List<BaseAdapter>
            {
                new CSharpAdapter()
            };

            _logService = new LogService();
            MessageHandlers = new List<MessageHandler>()
            {
                new GlobalInvocationHandler(ModuleAdapters)
            };
        }

        public void Start()
        {
            IsTerminated = false;

            _logService.LogInfo("Bridge ready, waiting for connection.");

            Server.WaitForConnection();

            _logService.LogInfo("Client connected.");

            while (!IsTerminated)
            {
                var messageHandled = false;
                var message = Server.WaitForMessage();

                if (message == null)
                {
                    _logService.LogError("Message Received, but could not be parsed.");
                    continue;
                }

                foreach(var handler in MessageHandlers)
                {
                    var resultMessage = handler.HandleMessages(message);
                    if(resultMessage != null)
                    {
                        _logService.LogInfo($"Message received, answer: {resultMessage}");
                        Server.WriteMessage(resultMessage);
                        messageHandled = true;
                        continue;
                    }
                }

                if(!messageHandled)
                    _logService.LogWarning("Message received, but was not handled.");
            }
        }

        public void Stop()
        {
            IsTerminated = true;
        }

    }
}
