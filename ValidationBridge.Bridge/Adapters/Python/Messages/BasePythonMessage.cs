using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Bridge.Adapters.Python.Enumerations;

namespace ValidationBridge.Bridge.Adapters.Python.Messages
{
    public abstract class BaseMessage
    {
        public EPythonMessageType MessageType { get; set; }

        public abstract byte[] GetBytes();
    }
}
