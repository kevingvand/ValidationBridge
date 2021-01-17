using System.Collections.Generic;
using System.IO;
using System.Linq;
using ValiationBridge.Bridge.Services;

namespace ValiationBridge.Bridge.Adapters.CSharp
{
    public class CSharpAdapter : BaseAdapter
    {
        private LogService _logService;

        public CSharpAdapter()
        {
            _logService = new LogService();
        }

        public override List<string> LoadModule(string modulePath)
        {
            var fileExtension = Path.GetExtension(modulePath);
            if (!fileExtension.Equals(".dll")) return new List<string>();

            var catalog = new AdapterCatalog();
            catalog.ComposeFromAssembly(modulePath);

            foreach (var loadedModule in catalog.Modules)
            {
                var moduleName = loadedModule.GetName();
                if (LoadedModules.ContainsKey(loadedModule.GetName()))
                {
                    _logService.LogWarning($@"C# module with name: ""{moduleName}"" already exists, replacing.");
                    LoadedModules[moduleName] = loadedModule.GetType();
                }
                else LoadedModules.Add(moduleName, loadedModule.GetType());
            }

            return catalog.Modules.Select(module => module.GetName()).ToList();
        }
    }
}
