using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common.Messages;

namespace ValiationBridge.Bridge.Handlers
{
    public abstract class MessageHandler
    {
        public abstract PipeMessage HandleMessages(PipeMessage message);
    }
}
