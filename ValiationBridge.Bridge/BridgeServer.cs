using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common;

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
    }
}
