using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
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

        }

        public void Connect()
        {
            var isServerRunning = Directory.GetFiles(@"\\.\pipe\").Contains($@"\\.\pipe\{Constants.ServerName}");

            if (!isServerRunning)
                Process.Start(new ProcessStartInfo { FileName = Constants.BridgePath, WindowStyle = Constants.ShowBridgeWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden });

            ClientStream.Connect();
        }

        public PipeMessage WriteMessage(PipeMessage message)
        {
            //TODO: timeout
            //TODO: errorhandling

            Stream.Write(message.GetBytes());
            var returnBytes = Stream.Read();

            if (returnBytes.Length <= 0)
                return null;

            var messageType = (EMessageType)returnBytes[0];

            if (messageType == EMessageType.ERROR)
                return ErrorMessage.CreateFromBytes(returnBytes);

            if (messageType != EMessageType.RESULT)
                throw new System.Exception("Expected only result messages."); //TODO: better error ? logging?

            return ResultMessage.CreateFromBytes(returnBytes);
        }
    }
}
