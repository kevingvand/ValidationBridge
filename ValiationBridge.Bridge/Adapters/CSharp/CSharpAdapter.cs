using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValiationBridge.Bridge.Adapters.CSharp
{
    public class CSharpAdapter : BaseAdapter
    {
        public override List<string> LoadModule(string modulePath)
        {
            var fileExtension = Path.GetExtension(modulePath);
            if (!fileExtension.Equals(".dll")) return new List<string>();

            var catalog = new AdapterCatalog();
            catalog.ComposeFromAssembly(modulePath);

            var loadedModules = catalog.Modules.ToDictionary(module => module.GetName(), module => module.GetType());

            //TODO: what to do if 2 modules with the same name are loaded.

            LoadedModules = LoadedModules.Concat(loadedModules).ToDictionary(loadedModule => loadedModule.Key, loadedModule => loadedModule.Value);

            return catalog.Modules.Select(module => module.GetName()).ToList();
        }
    }
}
