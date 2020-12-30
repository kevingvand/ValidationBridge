using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImpromptuInterface;

namespace ValidationBridge.Invoker.Proxy
{
    public class ModuleProxy
    {
        private ICollection<KeyValuePair<string, object>> _members;
        public ExpandoObject Instance { get; set; }

        public ModuleProxy()
        {
            Instance = new ExpandoObject();
            _members = Instance;
        }

        public void AddMember(string name, object value)
        {
            _members.Add(new KeyValuePair<string, object>(name, value));
        }

        public TModule GetModule<TModule>()
        {
            return Instance.ActLike(typeof(TModule));
        }
    }
}
