using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using ValidationBridge.Common.Interfaces.Modules;

namespace ValidationBridge.Bridge.Adapters.CSharp
{
    internal class AdapterCatalog
    {
        [ImportMany]
        public IEnumerable<IModule> Modules { get; set; }

        public void ComposeFromAssembly(string assemblyPath)
        {
            //var catalog = new AggregateCatalog();
            var assembly = Assembly.LoadFrom(assemblyPath);
            var assemblyCatalog = new AssemblyCatalog(assembly);

            //catalog.Catalogs.Add(assemblyCatalog);

            var container = new CompositionContainer(assemblyCatalog);

            container.ComposeParts(this);
        }

        ///// The target collection for loaded module classes 
        //[ImportMany]
        //public IEnumerable<IModule> Modules { get; set; }

        ///// Load all classes implementing IModule from a specified assembly
        //public void ComposeFromAssembly(string assemblyPath)
        //{
        //    // Load the assembly from the specified path
        //    var assembly = Assembly.LoadFrom(assemblyPath);
        //    // Create a new assembly catalog and container, with the loaded assembly
        //    var assemblyCatalog = new AssemblyCatalog(assembly);
        //    var container = new CompositionContainer(assemblyCatalog);

        //    // Load the parts into target collection
        //    container.ComposeParts(this);
        //}
    }
}
