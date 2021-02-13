using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValiationBridge.Bridge.Services;

namespace ValiationBridge.Bridge.Adapters.Python
{
    public class PythonAdapter : BaseAdapter
    {
        private LogService _logService;

        public PythonAdapter()
        {
            _logService = new LogService();
        }

        public override List<string> LoadModule(string modulePath)
        {
            var python = new PythonConsole("LOAD_MODULES");

            if (!python.IsInstalled())
            {
                _logService.LogWarning($"Python is not installed or configured incorrectly, no Python modules can be loaded.");
                return new List<string>();
            }

            FileAttributes modulePathAttributes = File.GetAttributes(modulePath);

            if (modulePathAttributes.HasFlag(FileAttributes.Directory))
                //TODO: Load Modules From Directory
                //checkModuleScript = LoadModulesFromDirectory(modulePath);
            else if (Path.GetExtension(modulePath) == ".py")
                return new List<string>();
            //TODO: load modules from file.
            //checkModuleScript = LoadModuleFromFile(modulePath);
            else
                return new List<string>();

            python.Start();



            //TODO: craete python console for loading module

            python.Stop();

            return null;
        }

        public
    }
}
