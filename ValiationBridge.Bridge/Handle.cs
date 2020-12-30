using System;

namespace ValiationBridge.Bridge
{
    public class Handle
    {
        public Type HandleType { get; private set; }

        public object Instance { get; private set; }

        public Handle(Type handleType, object instance)
        {
            this.HandleType = handleType;
            this.Instance = instance;
        }
    }
}
