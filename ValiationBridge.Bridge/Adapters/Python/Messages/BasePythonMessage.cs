using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Adapters.Python.Enumerations;

namespace ValiationBridge.Bridge.Adapters.Python.Messages
{
    public abstract class BaseMessage
    {
        public EPythonMessageType MessageType { get; set; }

        public abstract byte[] GetBytes();
    }
}
