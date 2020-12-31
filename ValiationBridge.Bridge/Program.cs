using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Adapters;
using ValiationBridge.Bridge.Services;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Messages;

namespace ValiationBridge.Bridge
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: Move outside of Program.cs
            //TODO: multiple Server instances 
            var logService = new LogService();
            var server = new BridgeServer();
            var adapter = new CSharpAdapter();

            var instanceManager = new InstanceManager();

            logService.LogInfo("Bridge ready, waiting for connection.");

            server.WaitForConnection();

            logService.LogInfo("Client connected.");

            //TODO: create MessageHandler --> responsible for adapters and server communicaiton


            while(true)
            {
                var message = server.WaitForMessage();

                if(message == null)
                {
                    logService.LogError("Message Received, but could not be parsed.");
                    continue;
                }

                if(message.MessageType != EMessageType.INVOKE)
                {
                    logService.LogWarning("Message received, but no invocation was requested.");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                logService.LogInfo("Message received, parsing...");

                var invokeMessage = (InvokeMessage)message;

                if(invokeMessage.InstanceId == Guid.Empty)
                {
                    // Module management
                    var loadedModules = adapter.LoadedModules.Select(x => x.Name).ToArray();
                    var resultMessage = new ResultMessage(new Argument(loadedModules));
                    server.WriteMessage(resultMessage);
                    logService.LogInfo("Done parsing, result was transfered.");
                    continue;
                } 

                // Module Invocation
                //if (message.Equals("modules"))
                //server.Stream.WriteString();
            }

        }
    }
}
