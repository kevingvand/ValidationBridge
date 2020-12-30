using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common.Interfaces.Modules
{
    public interface IVoltageSource : IInstrument
    {
        void SetDCVoltage(double voltage);
        void SetACVoltage(double voltage);
    }
}
