using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationBridge.Common.Interfaces.Modules;

namespace InstrumentModules
{
    public class Keithley2000 : IVoltageSensor, IVoltageSource
    {
        public Guid InstanceId => Guid.Empty;

        public string GetName() => "Keithley2000";
        public string GetDescription() => "Keithley 2000 Multimeter module";

        private Random _random;

        private double maxDcRange = 20;
        private double maxAcRange = 120;

        public bool Connect(string connectionString)
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
            return connectionString.Contains("GPIB");
        }

        public Keithley2000()
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
        }

        public double GetACVoltage()
        {
            return _random.NextDouble() + _random.Next(20, (int)maxAcRange);
        }

        public double GetDCVoltage()
        {
            return _random.NextDouble() + _random.Next(0, (int)maxDcRange);
        }

        public void SetACVoltage(double voltage)
        {
            maxAcRange = voltage;
        }

        public void SetDCVoltage(double voltage)
        {
            maxDcRange = voltage;
        }
    }
}
