using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationBridge.Proxy
{
    public class ProxyModuleInformation
    {
        public string ClassName { get; set; }
        public string ModuleName { get; set; }
        public List<Type> Interfaces { get; set; }

        public ProxyModuleInformation(string className)
        {
            ClassName = className;
            Interfaces = new List<Type>();
        }

        public ProxyModuleInformation(string className, string moduleName, List<Type> interfaces)
            : this(className)
        {
            ModuleName = moduleName;
            Interfaces = interfaces;
        }
    }
}
