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
            LoadedModules = new List<IModule>();
        }

        public override IModule GetModule(string name)
        {
            return LoadedModules.FirstOrDefault(module => module.GetName().Equals(name));
        }

        public override List<string> LoadModule(string modulePath)
        {
            var fileExtension = Path.GetExtension(modulePath);
            if (!fileExtension.Equals(".dll")) return new List<string>();

            var catalog = new AdapterCatalog();
            catalog.ComposeFromAssembly(modulePath);

            LoadedModules.AddRange(catalog.Modules);

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
