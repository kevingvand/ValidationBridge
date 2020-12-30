using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValiationBridge.Bridge
{
    public class InstanceManager
    {
        public Dictionary<Guid, Handle> Instances { get; set; }

        public InstanceManager()
        {
            Instances = new Dictionary<Guid, Handle>();
        }

        public Guid CreateInstance(Type instanceType)
        {
            var instanceId = Guid.NewGuid();
            var instance = Activator.CreateInstance(instanceType);

            Instances.Add(instanceId, new Handle(instanceType, instance));

            return instanceId;
        }

        public void CleanInstance(Guid instance)
        {
            if (Instances.ContainsKey(instance))
                Instances.Remove(instance);
        }
    }
}
