using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common.Interfaces.Modules
{
    public interface IInstrument : IModule
    {
        // Test
        bool Connect(string connectionString);

        /// Another test
        bool ConnectGPIB(int gpibPort, int gpibIndex);
    }
}
