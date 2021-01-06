using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common.Interfaces.Modules
{
    public interface IInstrument : IModule
    {
        bool Connect(string connectionString);

        bool ConnectGPIB(int gpibPort, int gpibIndex);
    }
}
