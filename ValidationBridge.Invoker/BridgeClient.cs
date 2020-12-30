using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common;

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
    }
}
