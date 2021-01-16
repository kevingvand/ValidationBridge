using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValiationBridge.Bridge.Adapters
{
    public class CSharpAdapter : BaseAdapter
    {
        public CSharpAdapter()
            : base()
        {
            LoadedModules = new Dictionary<string, Type>();
        }

        public override IModule GetModule(string name)
        {
            if (!LoadedModules.ContainsKey(name))
                return null;

            var moduleType = LoadedModules[name];
            return (IModule) Activator.CreateInstance(moduleType);
        }

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

        private class AdapterCatalog
        {
            [ImportMany]
            public IEnumerable<IModule> Modules { get; set; }

            public void ComposeFromAssembly(string assemblyPath)
            {
                var catalog = new AggregateCatalog();
                var assembly = Assembly.LoadFrom(assemblyPath);
                var assemblyCatalog = new AssemblyCatalog(assembly);

                catalog.Catalogs.Add(assemblyCatalog);

                var container = new CompositionContainer(assemblyCatalog);

                container.ComposeParts(this);
            }
        }
    }
}
