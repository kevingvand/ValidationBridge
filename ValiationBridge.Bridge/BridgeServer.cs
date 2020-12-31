using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Messages;

namespace ValiationBridge.Bridge
{
    public class BridgeServer
    {
        public ValidationBridge.Common.PipeStream Stream { get; set; }
        public NamedPipeServerStream ServerStream { get; set; }

        public BridgeServer()
        {
            ServerStream = new NamedPipeServerStream(Constants.ServerName, PipeDirection.InOut, 1);
            Stream = new ValidationBridge.Common.PipeStream(ServerStream);
        }

        public void WaitForConnection()
        {
            ServerStream.WaitForConnection();
        }

        public PipeMessage WaitForMessage()
        {
            var bytes = Stream.Read();

            if (bytes.Length <= 0) return null;

            var messageType = (EMessageType)bytes[0];

            switch (messageType)
            {
                case EMessageType.INVOKE:
                    return InvokeMessage.CreateFromBytes(bytes);
                case EMessageType.RESULT:
                    return ResultMessage.CreateFromBytes(bytes);
                default:
                    return null;
            }
        }

        public void WriteMessage(PipeMessage message)
        {
            Stream.Write(message.GetBytes());
        }
    }
}
