using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common.Interfaces.Modules
{
    public interface IPlotter : IModule
    {
        double[] GetPoints(int count);
    }
}
