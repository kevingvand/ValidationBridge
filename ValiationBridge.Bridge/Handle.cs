using System;

namespace ValiationBridge.Bridge
{
    public class Handle
    {
        public Type HandleType { get; private set; }

        public object Instance { get; private set; }

        public Handle(object instance)
        {
            this.HandleType = instance.GetType();
            this.Instance = instance;
        }
        public Handle(Type handleType, object instance)
        {
            this.HandleType = handleType;
            this.Instance = instance;
        }
    }
}
