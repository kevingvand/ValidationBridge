using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common.Interfaces.Modules;

namespace InstrumentModules
{
    [Export(typeof(IModule))]
    public class Keithley2400 : IVoltageSource, IVoltageSensor
    {
        public Guid InstanceId => Guid.Empty;

        public string GetName() => "Keithley2400";
        public string GetDescription() => "Keithley 2400 Source Meter Module";

        private double _acVoltage = 1.0;
        private double _dcVoltage = 2.0;

        public bool Connect(string connectionString)
        {
            return true;
        }

        public bool ConnectGPIB(int gpibPort, int gpibIndex)
        {
            return false;
        }

        public double GetACVoltage()
        {
            return _acVoltage;
        }

        public double GetDCVoltage()
        {
            return _dcVoltage;
        }


        public void SetACVoltage(double voltage)
        {
            _acVoltage = voltage;
        }

        public void SetDCVoltage(double voltage)
        {
            _dcVoltage = voltage;
        }
    }
}
