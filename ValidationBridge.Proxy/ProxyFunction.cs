using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationBridge.Proxy
{
    public delegate object ProxyFunction(string instanceId, string functionName, params object[] parameters);
}
