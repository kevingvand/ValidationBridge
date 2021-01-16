% KeysightB2901A tester
x = Instruments.KeysightB2901A();
x.GetName
x.GetDCVoltage()
x = x.SetDCVoltage(1.24); % Note that matlab only uses value --> so the instance should be replaced after each call
x.GetDCVoltage()
