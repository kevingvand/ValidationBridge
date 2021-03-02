using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationBridge.Bridge.Adapters.Python
{
    public static class PythonInstanceManager
    {
        private static Dictionary<string, PythonConsole> _instances;

        public static PythonConsole GetInstance(string instanceId)
        {
            if (_instances == null) _instances = new Dictionary<string, PythonConsole>();

            if (_instances.ContainsKey(instanceId)) return _instances[instanceId];

            var instance = new PythonConsole(instanceId);
            instance.Start();

            _instances.Add(instanceId, instance);
            return instance;
        }

        public static void DestroyInstance(string instanceId)
        {
            if(_instances?.ContainsKey(instanceId) ?? false)
            {
                var instance = _instances[instanceId];
                instance.Stop();
            }
        }
    }
}
