using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValiationBridge.Bridge.Adapters.CSharp
{
    internal class AdapterCatalog
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
