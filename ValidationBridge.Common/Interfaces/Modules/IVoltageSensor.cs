using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationBridge.Common.Interfaces.Modules
{
    public interface IVoltageSensor : IInstrument
    {
        double GetDCVoltage();
        double GetACVoltage();
    }
}
