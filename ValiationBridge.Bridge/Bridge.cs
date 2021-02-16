using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Adapters;
using ValiationBridge.Bridge.Adapters.CSharp;
using ValiationBridge.Bridge.Adapters.Matlab;
using ValiationBridge.Bridge.Adapters.Python;
using ValiationBridge.Bridge.Handlers;
using ValiationBridge.Bridge.Services;
using ValidationBridge.Common.Messages;

namespace ValiationBridge.Bridge
{
    public class Bridge
    {
        private LogService _logService { get; set; }


        public bool IsTerminated { get; set; }

        public BridgeServer Server { get; set; }

        public InstanceManager InstanceManager { get; set; }

        public List<BaseAdapter> ModuleAdapters { get; set; }
        public List<MessageHandler> MessageHandlers { get; set; }

        public Bridge()
        {
            InstanceManager = new InstanceManager();
            //TODO: load dynamically?
            ModuleAdapters = new List<BaseAdapter>
            {
                new CSharpAdapter(),
                new MatlabAdapter(),
                new PythonAdapter()
            };

            _logService = new LogService();
            MessageHandlers = new List<MessageHandler>()
            {
                new GlobalInvocationHandler(InstanceManager, ModuleAdapters),
                new ModuleInvocationHandler(InstanceManager),
            };
        }

        public void Start()
        {
            if (Server != null)
                Server.ServerStream.Close();

            Server = new BridgeServer();

            IsTerminated = false;

            _logService.LogInfo("Bridge ready, waiting for connection.");

            Server.WaitForConnection();

            _logService.LogInfo("Client connected.");

            while (!IsTerminated)
            {
                var messageHandled = false;
                var message = Server.WaitForMessage();

                if(!Server.IsConnected)
                {
                    _logService.LogError("Connection terminated from other end.");
                    Start();
                    return;
                }

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
                        if (resultMessage.MessageType == ValidationBridge.Common.Enumerations.EMessageType.ERROR)
                            _logService.LogError(((ErrorMessage)resultMessage).Message);
                        else
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
