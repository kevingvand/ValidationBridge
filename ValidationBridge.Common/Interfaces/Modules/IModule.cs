using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common.Interfaces.Modules
{
    public interface IModule
    {
        Guid InstanceId { get; }

        string GetName();
        string GetDescription();
    }
}
