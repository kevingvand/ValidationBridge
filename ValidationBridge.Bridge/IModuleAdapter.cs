using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValidationBridge.Bridge
{
    /// Base interface for every module adapter, entry points for module communication
    public interface IModuleAdapter
    {
        /// A dictionary of all dynamically created module types, mapped by their names
        Dictionary<string, Type> LoadedModules { get; set; }

        /// Instantiates a new instance of the module type with the specific name
        IModule GetModule(string name);

        /// Loads all valid module types from the specified path
        Dictionary<string, Type> LoadModules(string modulePath);
    }
}
