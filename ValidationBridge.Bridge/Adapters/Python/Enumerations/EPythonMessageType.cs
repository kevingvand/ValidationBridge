using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationBridge.Bridge.Adapters.Python.Enumerations
{
    public enum EPythonMessageType
    {
        EVALUATE = 1,
        EXECUTE = 2,
        RESULT = 3,
        ERROR = 4,
        NONE = 100
    }
}
