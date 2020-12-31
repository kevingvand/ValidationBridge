using System.IO.Pipes;
using ValidationBridge.Common;
using ValidationBridge.Common.Enumerations;
using ValidationBridge.Common.Messages;

namespace ValidationBridge.Invoker
{
    public class BridgeClient
    {
        public Common.PipeStream Stream { get; set; }
        public NamedPipeClientStream ClientStream { get; set; }

        public BridgeClient()
        {
            ClientStream = new NamedPipeClientStream(".", Constants.ServerName, PipeDirection.InOut, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.Impersonation);
            Stream = new Common.PipeStream(ClientStream);

            ClientStream.Connect();
        }

        public ResultMessage WriteMessage(PipeMessage message)
        {
            //TODO: timeout
            //TODO: errorhandling

            Stream.Write(message.GetBytes());
            var returnBytes = Stream.Read();

            if (returnBytes.Length <= 0 || (EMessageType)returnBytes[0] != EMessageType.RESULT)
                return null;

            return ResultMessage.CreateFromBytes(returnBytes);
        }
    }
}
